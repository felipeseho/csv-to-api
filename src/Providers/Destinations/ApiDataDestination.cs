using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using n2n.Core;

namespace n2n.Providers.Destinations;

/// <summary>
///     Destino de dados API REST
/// </summary>
public class ApiDataDestination : DataDestinationBase
{
    private HttpClient? _httpClient;
    private string _endpointUrl = string.Empty;
    private string _method = "POST";
    private int _timeout = 30;
    private int _retryAttempts = 3;
    private int _retryDelaySeconds = 2;
    private int? _maxRequestsPerSecond;
    private Dictionary<string, string> _headers = new();
    private Dictionary<string, string> _fieldMapping = new();
    private SemaphoreSlim? _rateLimiter;
    private Timer? _rateLimitTimer;

    public override string DestinationType => "API";

    protected override Task OnInitializeAsync()
    {
        _endpointUrl = GetConfigValue<string>("EndpointUrl", string.Empty);
        _method = GetConfigValue<string>("Method", "POST").ToUpper();
        _timeout = GetConfigValue<int>("Timeout", 30);
        _retryAttempts = GetConfigValue<int>("RetryAttempts", 3);
        _retryDelaySeconds = GetConfigValue<int>("RetryDelaySeconds", 2);
        _maxRequestsPerSecond = GetConfigValue<int?>("MaxRequestsPerSecond", null);

        // Headers
        if (Configuration.TryGetValue("Headers", out var headersObj) &&
            headersObj is Dictionary<string, object> headersDict)
        {
            _headers = headersDict.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty);
        }

        // Field mapping
        if (Configuration.TryGetValue("FieldMapping", out var mappingObj) &&
            mappingObj is Dictionary<string, object> mappingDict)
        {
            _fieldMapping = mappingDict.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty);
        }

        // Criar HttpClient
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_timeout)
        };

        // Adicionar headers (exceto Content-Type que vai no request)
        var contentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Content-Type", "Content-Length", "Content-Encoding", "Content-Language",
            "Content-Location", "Content-MD5", "Content-Range", "Expires", "Last-Modified"
        };

        foreach (var header in _headers.Where(h => !contentHeaders.Contains(h.Key)))
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        // Configurar rate limiting se necessário
        if (_maxRequestsPerSecond.HasValue && _maxRequestsPerSecond.Value > 0)
        {
            _rateLimiter = new SemaphoreSlim(_maxRequestsPerSecond.Value, _maxRequestsPerSecond.Value);
            _rateLimitTimer = new Timer(_ =>
            {
                try
                {
                    var currentCount = _rateLimiter.CurrentCount;
                    var tokensToRelease = _maxRequestsPerSecond.Value - currentCount;
                    if (tokensToRelease > 0)
                    {
                        _rateLimiter.Release(tokensToRelease);
                    }
                }
                catch (SemaphoreFullException)
                {
                    // Ignorar
                }
            }, null, 1000, 1000);
        }

        Timer.Start();
        return Task.CompletedTask;
    }

    protected override Task OnValidateConfigurationAsync(ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(_endpointUrl))
        {
            result.AddError("EndpointUrl é obrigatório");
        }
        else if (!Uri.TryCreate(_endpointUrl, UriKind.Absolute, out _))
        {
            result.AddError($"EndpointUrl inválida: {_endpointUrl}");
        }

        if (!new[] { "POST", "PUT", "PATCH" }.Contains(_method))
        {
            result.AddError($"Método HTTP '{_method}' não suportado. Use POST, PUT ou PATCH");
        }

        if (_timeout <= 0)
        {
            result.AddError("Timeout deve ser maior que 0");
        }

        if (_retryAttempts < 1)
        {
            result.AddError("RetryAttempts deve ser maior ou igual a 1");
        }

        return Task.CompletedTask;
    }

    public override async Task<WriteResult> WriteAsync(DataRecord record, CancellationToken cancellationToken = default)
    {
        if (_httpClient == null)
        {
            throw new InvalidOperationException("Destino não foi inicializado");
        }

        // Aguardar rate limiter se configurado
        if (_rateLimiter != null)
        {
            await _rateLimiter.WaitAsync(cancellationToken);
        }

        var requestTimer = Stopwatch.StartNew();

        try
        {
            // Mapear campos
            var payload = MapFields(record);

            // Serializar
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Tentar enviar com retry
            return await SendWithRetryAsync(json, record.RecordId, requestTimer, cancellationToken);
        }
        catch (Exception ex)
        {
            requestTimer.Stop();
            var result = WriteResult.FailureResult(ex.Message, 500, requestTimer.ElapsedMilliseconds);
            RecordResponse(requestTimer.ElapsedMilliseconds, false);
            return result;
        }
    }

    private Dictionary<string, object> MapFields(DataRecord record)
    {
        var payload = new Dictionary<string, object>();

        if (_fieldMapping.Count == 0)
        {
            // Sem mapeamento, usar dados diretos
            return record.Data;
        }

        // Aplicar mapeamento
        foreach (var mapping in _fieldMapping)
        {
            var targetField = mapping.Key;
            var sourceField = mapping.Value;

            // Verificar se é valor fixo (começa com =)
            if (sourceField.StartsWith("="))
            {
                payload[targetField] = sourceField.Substring(1);
            }
            else if (record.Data.TryGetValue(sourceField, out var value))
            {
                payload[targetField] = value;
            }
        }

        return payload;
    }

    private async Task<WriteResult> SendWithRetryAsync(string json, string recordId,
        Stopwatch requestTimer, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= _retryAttempts; attempt++)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Aplicar Content-Type customizado se configurado
                if (_headers.TryGetValue("Content-Type", out var contentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                }

                HttpResponseMessage response;
                if (_method == "POST")
                {
                    response = await _httpClient!.PostAsync(_endpointUrl, content, cancellationToken);
                }
                else if (_method == "PUT")
                {
                    response = await _httpClient!.PutAsync(_endpointUrl, content, cancellationToken);
                }
                else if (_method == "PATCH")
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), _endpointUrl)
                    {
                        Content = content
                    };
                    response = await _httpClient!.SendAsync(request, cancellationToken);
                }
                else
                {
                    throw new NotSupportedException($"Método HTTP '{_method}' não suportado");
                }

                requestTimer.Stop();

                var statusCode = (int)response.StatusCode;

                // Registrar métricas customizadas
                if (!Metrics.CustomMetrics.ContainsKey("StatusCodes"))
                {
                    Metrics.CustomMetrics["StatusCodes"] = new Dictionary<int, long>();
                }
                var statusCodes = (Dictionary<int, long>)Metrics.CustomMetrics["StatusCodes"];
                statusCodes[statusCode] = statusCodes.GetValueOrDefault(statusCode, 0) + 1;

                if (attempt > 1)
                {
                    Metrics.TotalRetries++;
                }

                if (!response.IsSuccessStatusCode)
                {
                    // Retry em erros 5xx ou timeout
                    if ((statusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout) &&
                        attempt < _retryAttempts)
                    {
                        await Task.Delay(_retryDelaySeconds * 1000, cancellationToken);
                        requestTimer.Restart();
                        continue;
                    }

                    var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = WriteResult.FailureResult(errorMessage, statusCode,
                        requestTimer.ElapsedMilliseconds);
                    RecordResponse(requestTimer.ElapsedMilliseconds, false);
                    return result;
                }

                RecordResponse(requestTimer.ElapsedMilliseconds, true);
                return WriteResult.SuccessResult(requestTimer.ElapsedMilliseconds);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < _retryAttempts)
                {
                    await Task.Delay(_retryDelaySeconds * 1000, cancellationToken);
                    requestTimer.Restart();
                }
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                if (attempt < _retryAttempts)
                {
                    await Task.Delay(_retryDelaySeconds * 1000, cancellationToken);
                    requestTimer.Restart();
                }
            }
        }

        // Todas tentativas falharam
        requestTimer.Stop();
        var failureResult = WriteResult.FailureResult(
            lastException?.Message ?? "Todas as tentativas falharam",
            500,
            requestTimer.ElapsedMilliseconds);
        RecordResponse(requestTimer.ElapsedMilliseconds, false);
        return failureResult;
    }

    public override async Task<BatchWriteResult> WriteBatchAsync(IEnumerable<DataRecord> records,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchWriteResult();
        var batchTimer = Stopwatch.StartNew();
        var recordsList = records.ToList();
        result.TotalRecords = recordsList.Count;

        if (_maxRequestsPerSecond.HasValue && _maxRequestsPerSecond.Value > 0)
        {
            var semaphore = new SemaphoreSlim(_maxRequestsPerSecond.Value, _maxRequestsPerSecond.Value);
            var resultsList = new System.Collections.Concurrent.ConcurrentBag<WriteResult>();

            try
            {
                var tasks = recordsList.Select(async record =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var writeResult = await WriteAsync(record, cancellationToken);
                        resultsList.Add(writeResult);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                result.IndividualResults.AddRange(resultsList);
                result.SuccessCount = resultsList.Count(r => r.Success);
                result.ErrorCount = resultsList.Count(r => !r.Success);
            }
            finally
            {
                semaphore.Dispose();
            }
        }
        else
        {
            var tasks = recordsList.Select(record => WriteAsync(record, cancellationToken));
            var results = await Task.WhenAll(tasks);

            result.IndividualResults.AddRange(results);
            result.SuccessCount = results.Count(r => r.Success);
            result.ErrorCount = results.Count(r => !r.Success);
        }

        batchTimer.Stop();
        result.TotalResponseTimeMs = batchTimer.ElapsedMilliseconds;

        return result;
    }

    public override void Dispose()
    {
        _rateLimitTimer?.Dispose();
        _rateLimiter?.Dispose();
        _httpClient?.Dispose();
        base.Dispose();
    }
}
