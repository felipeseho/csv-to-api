using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvToApi.Models;

namespace CsvToApi.Services;

/// <summary>
/// Serviço principal para processamento de arquivos CSV
/// </summary>
public class CsvProcessorService
{
    private readonly ValidationService _validationService;
    private readonly LoggingService _loggingService;
    private readonly ApiClientService _apiClientService;

    public CsvProcessorService(
        ValidationService validationService,
        LoggingService loggingService,
        ApiClientService apiClientService)
    {
        _validationService = validationService;
        _loggingService = loggingService;
        _apiClientService = apiClientService;
    }

    /// <summary>
    /// Processa arquivo CSV completo
    /// </summary>
    public async Task ProcessCsvFileAsync(Configuration config)
    {
        using var httpClient = _apiClientService.CreateHttpClient(config.Api);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = config.File.CsvDelimiter,
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(config.File.InputPath);
        using var csv = new CsvReader(reader, csvConfig);

        // Ler cabeçalho
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        if (headers == null || headers.Length == 0)
        {
            Console.WriteLine("Arquivo CSV não contém cabeçalho");
            return;
        }

        var batch = new List<CsvRecord>();
        var lineNumber = 1; // Linha 1 é o cabeçalho
        var totalProcessed = 0;
        var totalErrors = 0;
        var totalSkipped = 0;

        // Pular linhas até a linha inicial configurada
        while (lineNumber < config.File.StartLine && await csv.ReadAsync())
        {
            lineNumber++;
            totalSkipped++;
        }

        if (totalSkipped > 0)
        {
            Console.WriteLine($"⏭️  Puladas {totalSkipped} linhas (iniciando na linha {config.File.StartLine})");
        }

        while (await csv.ReadAsync())
        {
            lineNumber++;
            
            var record = new CsvRecord
            {
                LineNumber = lineNumber,
                Data = new Dictionary<string, string>()
            };

            foreach (var header in headers)
            {
                record.Data[header] = csv.GetField(header) ?? string.Empty;
            }

            // Validar campos
            var validationError = _validationService.ValidateRecord(record, config.File.Mapping);
            if (validationError != null)
            {
                await _loggingService.LogError(config.File.LogPath, record, 400, validationError, headers);
                totalErrors++;
                continue;
            }

            batch.Add(record);

            // Processar lote quando atingir o tamanho configurado
            if (batch.Count >= config.File.BatchLines)
            {
                var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, headers);
                totalProcessed += batch.Count;
                totalErrors += errors;
                batch.Clear();
                
                Console.WriteLine($"Processadas {totalProcessed} linhas. Erros: {totalErrors}");
            }
        }

        // Processar lote restante
        if (batch.Count > 0)
        {
            var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, headers);
            totalProcessed += batch.Count;
            totalErrors += errors;
            
            Console.WriteLine($"Processadas {totalProcessed} linhas. Erros: {totalErrors}");
        }

        Console.WriteLine($"\nTotal de linhas processadas: {totalProcessed}");
        Console.WriteLine($"Total de erros: {totalErrors}");
    }
}

