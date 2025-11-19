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
    private readonly CheckpointService _checkpointService;

    public CsvProcessorService(
        ValidationService validationService,
        LoggingService loggingService,
        ApiClientService apiClientService,
        CheckpointService checkpointService)
    {
        _validationService = validationService;
        _loggingService = loggingService;
        _apiClientService = apiClientService;
        _checkpointService = checkpointService;
    }

    /// <summary>
    /// Processa arquivo CSV completo
    /// </summary>
    public async Task ProcessCsvFileAsync(Configuration config, bool dryRun = false)
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

        // Tentar carregar checkpoint se configurado
        Checkpoint? checkpoint = null;
        var startLineFromCheckpoint = config.File.StartLine;
        
        if (!string.IsNullOrWhiteSpace(config.File.CheckpointPath))
        {
            checkpoint = _checkpointService.LoadCheckpoint(config.File.CheckpointPath);
            if (checkpoint != null)
            {
                Console.WriteLine($"📍 Checkpoint encontrado! Retomando da linha {checkpoint.LastProcessedLine + 1}");
                Console.WriteLine($"   Progresso anterior: {checkpoint.SuccessCount} sucessos, {checkpoint.ErrorCount} erros");
                startLineFromCheckpoint = checkpoint.LastProcessedLine + 1;
            }
        }

        var batch = new List<CsvRecord>();
        var lineNumber = 1; // Linha 1 é o cabeçalho
        var totalProcessed = checkpoint?.TotalProcessed ?? 0;
        var totalErrors = checkpoint?.ErrorCount ?? 0;
        var totalSuccess = checkpoint?.SuccessCount ?? 0;
        var totalSkipped = 0;

        // Pular linhas até a linha inicial configurada
        while (lineNumber < startLineFromCheckpoint && await csv.ReadAsync())
        {
            lineNumber++;
            totalSkipped++;
        }

        if (totalSkipped > 0)
        {
            Console.WriteLine($"⏭️  Puladas {totalSkipped} linhas (iniciando na linha {startLineFromCheckpoint})");
        }

        var lastCheckpointSave = DateTime.Now;
        var checkpointIntervalSeconds = 30; // Salvar checkpoint a cada 30 segundos

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
                var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, headers, dryRun);
                totalProcessed += batch.Count;
                totalErrors += errors;
                totalSuccess += (batch.Count - errors);
                batch.Clear();
                
                Console.WriteLine($"Processadas {totalProcessed} linhas. Sucessos: {totalSuccess}, Erros: {totalErrors}");

                // Salvar checkpoint periodicamente
                if (!string.IsNullOrWhiteSpace(config.File.CheckpointPath) && 
                    (DateTime.Now - lastCheckpointSave).TotalSeconds >= checkpointIntervalSeconds)
                {
                    await _checkpointService.SaveCheckpointAsync(
                        config.File.CheckpointPath, 
                        lineNumber, 
                        totalProcessed, 
                        totalSuccess, 
                        totalErrors);
                    lastCheckpointSave = DateTime.Now;
                }
            }
        }

        // Processar lote restante
        if (batch.Count > 0)
        {
            var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, headers, dryRun);
            totalProcessed += batch.Count;
            totalErrors += errors;
            totalSuccess += (batch.Count - errors);
            
            Console.WriteLine($"Processadas {totalProcessed} linhas. Sucessos: {totalSuccess}, Erros: {totalErrors}");
        }

        // Salvar checkpoint final
        if (!string.IsNullOrWhiteSpace(config.File.CheckpointPath))
        {
            await _checkpointService.SaveCheckpointAsync(
                config.File.CheckpointPath, 
                lineNumber, 
                totalProcessed, 
                totalSuccess, 
                totalErrors);
            
            Console.WriteLine($"\n💾 Checkpoint salvo em: {config.File.CheckpointPath}");
        }

        Console.WriteLine($"\n📊 Resumo do Processamento:");
        Console.WriteLine($"   Total de linhas processadas: {totalProcessed}");
        Console.WriteLine($"   ✅ Sucessos: {totalSuccess} ({(totalProcessed > 0 ? (totalSuccess * 100.0 / totalProcessed).ToString("F1") : "0")}%)");
        Console.WriteLine($"   ❌ Erros: {totalErrors} ({(totalProcessed > 0 ? (totalErrors * 100.0 / totalProcessed).ToString("F1") : "0")}%)");
    }
}

