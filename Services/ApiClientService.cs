using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CsvToApi.Models;
using CsvToApi.Utils;

namespace CsvToApi.Services;

/// <summary>
/// Serviço para comunicação com API REST
/// </summary>
public class ApiClientService
{
    private readonly LoggingService _loggingService;
    private readonly MetricsService? _metricsService;
    private readonly SemaphoreSlim? _rateLimiter;
    private readonly Timer? _rateLimitTimer;

    public ApiClientService(LoggingService loggingService, NamedEndpoint endpointConfig, MetricsService? metricsService = null)
    {
        _loggingService = loggingService;
        _metricsService = metricsService;
        
        // Configurar rate limiting se especificado
        if (endpointConfig.MaxRequestsPerSecond.HasValue && endpointConfig.MaxRequestsPerSecond.Value > 0)
        {
            _rateLimiter = new SemaphoreSlim(0, endpointConfig.MaxRequestsPerSecond.Value);
            _rateLimitTimer = new Timer(_ =>
            {
                try
                {
                    // Liberar tokens a cada segundo
                    var currentCount = _rateLimiter.CurrentCount;
                    var tokensToRelease = endpointConfig.MaxRequestsPerSecond.Value - currentCount;
                    if (tokensToRelease > 0)
                    {
                        _rateLimiter.Release(tokensToRelease);
                    }
                }
                catch (SemaphoreFullException)
                {
                    // Ignorar se já estiver cheio
                }
            }, null, 0, 1000);
        }
    }

    /// <summary>
    /// Cria e configura o HttpClient
    /// </summary>
    public HttpClient CreateHttpClient(NamedEndpoint endpointConfig)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(endpointConfig.RequestTimeout)
        };

        // Configurar headers customizados
        // Nota: Content-Type e outros headers de conteúdo devem ser configurados no HttpContent, não no HttpClient
        if (endpointConfig.Headers != null && endpointConfig.Headers.Count > 0)
        {
            var contentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Content-Type", "Content-Length", "Content-Encoding", "Content-Language", 
                "Content-Location", "Content-MD5", "Content-Range", "Expires", "Last-Modified"
            };

            foreach (var header in endpointConfig.Headers)
            {
                // Ignorar headers de conteúdo (eles serão adicionados no HttpContent quando a requisição for feita)
                if (!contentHeaders.Contains(header.Key))
                {
                    httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        return httpClient;
    }

    /// <summary>
    /// Processa um lote de registros
    /// </summary>
    public async Task<int> ProcessBatchAsync(HttpClient httpClient, List<CsvRecord> batch, 
        Configuration config, string logPath, string[] headers, bool dryRun = false, string? commandLineEndpointName = null)
    {
        // Processar em paralelo para melhor performance
        var tasks = batch.Select(record => ProcessRecordAsync(httpClient, record, config, logPath, headers, dryRun, commandLineEndpointName));
        var results = await Task.WhenAll(tasks);

        return results.Count(r => !r);
    }

    /// <summary>
    /// Processa um único registro
    /// </summary>
    private async Task<bool> ProcessRecordAsync(HttpClient httpClient, CsvRecord record, 
        Configuration config, string logPath, string[] headers, bool dryRun = false, string? commandLineEndpointName = null)
    {
        // Aguardar rate limiter se configurado
        if (_rateLimiter != null)
        {
            await _rateLimiter.WaitAsync();
        }

        try
        {
            // Determinar qual endpoint usar
            var endpointName = commandLineEndpointName; // Prioridade 1: Argumento linha de comando
            
            // Prioridade 2: Coluna CSV (se configurada)
            if (string.IsNullOrWhiteSpace(endpointName) && !string.IsNullOrWhiteSpace(config.EndpointColumnName))
            {
                if (record.Data.TryGetValue(config.EndpointColumnName, out var csvEndpointName))
                {
                    endpointName = csvEndpointName;
                }
            }
            
            // Selecionar configuração de API apropriada
            var apiConfig = GetEndpointConfiguration(config, endpointName);
            
            // Construir payload da API
            var payload = PayloadBuilder.BuildApiPayload(record, apiConfig.Mapping);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            });

            // Modo dry run: apenas simula o envio
            if (dryRun)
            {
                var endpointInfo = string.IsNullOrWhiteSpace(endpointName) ? "default" : endpointName;
                Console.WriteLine($"[DRY RUN] Linha {record.LineNumber} [endpoint: {endpointInfo}]: {json}");
                return true;
            }

            return await SendWithRetryAsync(httpClient, apiConfig, logPath, json, record, headers);
        }
        catch (Exception ex)
        {
            await _loggingService.LogError(logPath, record, 500, ex.Message, headers);
            return false;
        }
    }
    
    /// <summary>
    /// Retorna a configuração do endpoint apropriado
    /// </summary>
    private NamedEndpoint GetEndpointConfiguration(Configuration config, string? endpointName)
    {
        // Se não há nome de endpoint especificado, usar endpoint padrão configurado
        if (string.IsNullOrWhiteSpace(endpointName))
        {
            if (!string.IsNullOrWhiteSpace(config.DefaultEndpoint))
            {
                endpointName = config.DefaultEndpoint;
            }
            else if (config.Endpoints.Count == 1)
            {
                // Se há apenas um endpoint, usar ele
                return config.Endpoints[0];
            }
            else
            {
                throw new InvalidOperationException(
                    "Nome do endpoint não especificado. Use --endpoint-name, configure 'endpointColumnName' no CSV, " +
                    "ou defina 'defaultEndpoint' na configuração. " +
                    $"Endpoints disponíveis: {string.Join(", ", config.Endpoints.Select(e => e.Name))}");
            }
        }
        
        // Buscar endpoint pelo nome
        var endpoint = config.Endpoints.FirstOrDefault(e => 
            e.Name.Equals(endpointName, StringComparison.OrdinalIgnoreCase));
        
        if (endpoint == null)
        {
            throw new InvalidOperationException(
                $"Endpoint '{endpointName}' não encontrado na configuração. " +
                $"Endpoints disponíveis: {string.Join(", ", config.Endpoints.Select(e => e.Name))}");
        }
        
        return endpoint;
    }

    /// <summary>
    /// Envia requisição com retry policy
    /// </summary>
    private async Task<bool> SendWithRetryAsync(HttpClient httpClient, NamedEndpoint endpointConfig, 
        string logPath, string json, CsvRecord record, string[] headers)
    {
        int attempts = 0;
        Exception? lastException = null;
        var requestTimer = Stopwatch.StartNew();

        while (attempts < endpointConfig.RetryAttempts)
        {
            attempts++;
            
            try
            {
                // Criar conteúdo da requisição
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                // Aplicar Content-Type customizado se configurado
                if (endpointConfig.Headers != null && endpointConfig.Headers.ContainsKey("Content-Type"))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(endpointConfig.Headers["Content-Type"]);
                }

                HttpResponseMessage response;
                if (endpointConfig.Method.ToUpper() == "POST")
                {
                    response = await httpClient.PostAsync(endpointConfig.EndpointUrl, content);
                }
                else if (endpointConfig.Method.ToUpper() == "PUT")
                {
                    response = await httpClient.PutAsync(endpointConfig.EndpointUrl, content);
                }
                else
                {
                    throw new NotSupportedException($"Método HTTP '{endpointConfig.Method}' não suportado");
                }

                requestTimer.Stop();
                
                // Registrar métricas
                _metricsService?.RecordResponseTime(requestTimer.ElapsedMilliseconds);
                _metricsService?.RecordHttpStatusCode((int)response.StatusCode);
                
                if (attempts > 1)
                {
                    _metricsService?.RecordRetry();
                }

                if (!response.IsSuccessStatusCode)
                {
                    // Se for erro do servidor (5xx) ou timeout, tentar novamente
                    if ((int)response.StatusCode >= 500 || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        if (attempts < endpointConfig.RetryAttempts)
                        {
                            Console.WriteLine($"Tentativa {attempts}/{endpointConfig.RetryAttempts} falhou (HTTP {(int)response.StatusCode}). Aguardando {endpointConfig.RetryDelaySeconds}s...");
                            await Task.Delay(endpointConfig.RetryDelaySeconds * 1000);
                            requestTimer.Restart();
                            continue;
                        }
                    }

                    var errorMessage = await response.Content.ReadAsStringAsync();
                    await _loggingService.LogError(logPath, record, (int)response.StatusCode, 
                        errorMessage, headers);
                    _metricsService?.RecordError();
                    return false;
                }

                _metricsService?.RecordSuccess();
                return true;
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempts < endpointConfig.RetryAttempts)
                {
                    Console.WriteLine($"Tentativa {attempts}/{endpointConfig.RetryAttempts} falhou ({ex.Message}). Aguardando {endpointConfig.RetryDelaySeconds}s...");
                    await Task.Delay(endpointConfig.RetryDelaySeconds * 1000);
                    requestTimer.Restart();
                    continue;
                }
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                if (attempts < endpointConfig.RetryAttempts)
                {
                    Console.WriteLine($"Tentativa {attempts}/{endpointConfig.RetryAttempts} timeout. Aguardando {endpointConfig.RetryDelaySeconds}s...");
                    await Task.Delay(endpointConfig.RetryDelaySeconds * 1000);
                    requestTimer.Restart();
                    continue;
                }
            }
        }

        // Todas as tentativas falharam
        await _loggingService.LogError(logPath, record, 500, 
            lastException?.Message ?? "Todas as tentativas falharam", headers);
        _metricsService?.RecordError();
        return false;
    }

    public void Dispose()
    {
        _rateLimitTimer?.Dispose();
        _rateLimiter?.Dispose();
    }
}

