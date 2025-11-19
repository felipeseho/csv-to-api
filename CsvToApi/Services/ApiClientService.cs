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

    public ApiClientService(LoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    /// <summary>
    /// Cria e configura o HttpClient
    /// </summary>
    public HttpClient CreateHttpClient(ApiConfiguration apiConfig)
    {
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(apiConfig.RequestTimeout)
        };

        // Configurar autenticação
        if (!string.IsNullOrWhiteSpace(apiConfig.AuthToken))
        {
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", apiConfig.AuthToken);
        }

        return httpClient;
    }

    /// <summary>
    /// Processa um lote de registros
    /// </summary>
    public async Task<int> ProcessBatchAsync(HttpClient httpClient, List<CsvRecord> batch, 
        Configuration config, string[] headers)
    {
        // Processar em paralelo para melhor performance
        var tasks = batch.Select(record => ProcessRecordAsync(httpClient, record, config, headers));
        var results = await Task.WhenAll(tasks);

        return results.Count(r => !r);
    }

    /// <summary>
    /// Processa um único registro
    /// </summary>
    private async Task<bool> ProcessRecordAsync(HttpClient httpClient, CsvRecord record, 
        Configuration config, string[] headers)
    {
        try
        {
            // Construir payload da API
            var payload = PayloadBuilder.BuildApiPayload(record, config.Api.Mapping);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (config.Api.Method.ToUpper() == "POST")
            {
                response = await httpClient.PostAsync(config.Api.EndpointUrl, content);
            }
            else if (config.Api.Method.ToUpper() == "PUT")
            {
                response = await httpClient.PutAsync(config.Api.EndpointUrl, content);
            }
            else
            {
                throw new NotSupportedException($"Método HTTP '{config.Api.Method}' não suportado");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                await _loggingService.LogError(config.File.LogPath, record, (int)response.StatusCode, 
                    errorMessage, headers);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogError(config.File.LogPath, record, 500, ex.Message, headers);
            return false;
        }
    }
}

