using System.Diagnostics;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvToApi.Models;
using Spectre.Console;

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
    private readonly MetricsService _metricsService;
    private FilterService? _filterService;

    public CsvProcessorService(
        ValidationService validationService,
        LoggingService loggingService,
        ApiClientService apiClientService,
        CheckpointService checkpointService,
        MetricsService metricsService)
    {
        _validationService = validationService;
        _loggingService = loggingService;
        _apiClientService = apiClientService;
        _checkpointService = checkpointService;
        _metricsService = metricsService;
    }

    /// <summary>
    /// Processa arquivo CSV completo
    /// </summary>
    public async Task ProcessCsvFileAsync(Configuration config, ExecutionPaths executionPaths, bool dryRun = false, string? endpointName = null)
    {
        // Inicializar serviço de filtros a partir das colunas configuradas
        _filterService = new FilterService(config.File.Columns);
        
        // Exibir filtros configurados
        var filtersCount = config.File.Columns.Count(c => c.Filter != null);
        if (filtersCount > 0)
        {
            AnsiConsole.MarkupLine($"[cyan1]🔍 {_filterService.GetFiltersSummary()}[/]");
            AnsiConsole.WriteLine();
        }
        
        // Obter configuração do endpoint a usar (para criar HttpClient)
        var configService = new ConfigurationService();
        var endpointConfig = configService.GetEndpointConfiguration(config, endpointName);
        
        using var httpClient = _apiClientService.CreateHttpClient(endpointConfig);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = config.File.CsvDelimiter,
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        // Contar total de linhas primeiro
        int totalLines = 0;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan1"))
            .StartAsync("[cyan1]Contando linhas do arquivo...[/]", async ctx =>
            {
                await Task.Run(() =>
                {
                    totalLines = CountCsvLines(config.File.InputPath);
                    _metricsService.StartProcessing(totalLines);
                });
            });

        AnsiConsole.MarkupLine($"[cyan1]📊 Total de linhas no arquivo:[/] [yellow]{totalLines}[/]");
        AnsiConsole.WriteLine();

        using var reader = new StreamReader(config.File.InputPath);
        using var csv = new CsvReader(reader, csvConfig);

        // Ler cabeçalho
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        if (headers == null || headers.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]✗ Arquivo CSV não contém cabeçalho[/]");
            return;
        }

        // Tentar carregar checkpoint se configurado
        Checkpoint? checkpoint = null;
        var startLineFromCheckpoint = config.File.StartLine;
        
        if (!string.IsNullOrWhiteSpace(executionPaths.CheckpointPath))
        {
            checkpoint = _checkpointService.LoadCheckpoint(executionPaths.CheckpointPath);
            if (checkpoint != null)
            {
                AnsiConsole.MarkupLine($"[cyan1]📍 Checkpoint encontrado! Retomando da linha[/] [yellow]{checkpoint.LastProcessedLine + 1}[/]");
                AnsiConsole.MarkupLine($"[grey]   Progresso anterior: {checkpoint.SuccessCount} sucessos, {checkpoint.ErrorCount} erros[/]");
                startLineFromCheckpoint = checkpoint.LastProcessedLine + 1;
            }
        }

        var batch = new List<CsvRecord>();
        var lineNumber = 1; // Linha 1 é o cabeçalho
        var totalProcessed = checkpoint?.TotalProcessed ?? 0;
        var totalErrors = checkpoint?.ErrorCount ?? 0;
        var totalSuccess = checkpoint?.SuccessCount ?? 0;
        var totalSkipped = 0;
        var totalFiltered = 0; // Linhas filtradas

        // Pular linhas até a linha inicial configurada
        while (lineNumber < startLineFromCheckpoint && await csv.ReadAsync())
        {
            lineNumber++;
            totalSkipped++;
        }

        if (totalSkipped > 0)
        {
            AnsiConsole.MarkupLine($"[yellow]⏭️  Puladas {totalSkipped} linhas (iniciando na linha {startLineFromCheckpoint})[/]");
            _metricsService.RecordSkippedLines(totalSkipped);
        }

        var lastCheckpointSave = DateTime.Now;
        var checkpointIntervalSeconds = 30; // Salvar checkpoint a cada 30 segundos
        var linesProcessedCount = 0; // Contador de linhas processadas (sem contar as puladas)

        // Processar com barra de progresso
        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[cyan1]Processando CSV[/]", maxValue: config.File.MaxLines ?? totalLines);

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

                    // Aplicar filtros
                    if (_filterService != null && !_filterService.PassesFilters(record))
                    {
                        totalFiltered++;
                        _metricsService.RecordSkippedLines(1);
                        continue;
                    }

                    // Validar campos
                    var validationError = _validationService.ValidateRecord(record, config.File.Columns);
                    if (validationError != null)
                    {
                        await _loggingService.LogError(executionPaths.LogPath, record, 400, validationError, headers);
                        totalErrors++;
                        _metricsService.RecordValidationError();
                        continue;
                    }

                    batch.Add(record);
                    linesProcessedCount++; // Incrementar após adicionar ao batch

                    // Processar lote quando atingir o tamanho configurado OU quando atingir o limite máximo de linhas
                    var shouldProcessBatch = batch.Count >= config.File.BatchLines ||
                                            (config.File.MaxLines.HasValue && linesProcessedCount >= config.File.MaxLines.Value);
                    
                    if (shouldProcessBatch)
                    {
                        var batchTimer = Stopwatch.StartNew();
                        var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, executionPaths.LogPath, headers, dryRun, endpointName);
                        batchTimer.Stop();
                        
                        _metricsService.RecordBatchTime(batchTimer.ElapsedMilliseconds);
                        
                        totalProcessed += batch.Count;
                        totalErrors += errors;
                        totalSuccess += (batch.Count - errors);
                        
                        // Atualizar barra de progresso
                        task.Increment(batch.Count);
                        task.Description = $"[cyan1]Processando CSV[/] [grey]({totalSuccess} ✓ | {totalErrors} ✗)[/]";
                        
                        batch.Clear();

                        // Salvar checkpoint periodicamente
                        if (!string.IsNullOrWhiteSpace(executionPaths.CheckpointPath) && 
                            (DateTime.Now - lastCheckpointSave).TotalSeconds >= checkpointIntervalSeconds)
                        {
                            await _checkpointService.SaveCheckpointAsync(
                                executionPaths.CheckpointPath, 
                                lineNumber, 
                                totalProcessed, 
                                totalSuccess, 
                                totalErrors);
                            lastCheckpointSave = DateTime.Now;
                        }
                        
                        // Verificar se atingiu o limite máximo de linhas após processar o batch
                        if (config.File.MaxLines.HasValue && linesProcessedCount >= config.File.MaxLines.Value)
                        {
                            AnsiConsole.MarkupLine($"\n[green]✓ Limite de {config.File.MaxLines.Value} linhas processadas atingido. Encerrando...[/]");
                            break;
                        }
                    }
                }

                // Processar lote restante
                if (batch.Count > 0)
                {
                    var batchTimer = Stopwatch.StartNew();
                    var errors = await _apiClientService.ProcessBatchAsync(httpClient, batch, config, executionPaths.LogPath, headers, dryRun, endpointName);
                    batchTimer.Stop();
                    
                    _metricsService.RecordBatchTime(batchTimer.ElapsedMilliseconds);
                    
                    totalProcessed += batch.Count;
                    totalErrors += errors;
                    totalSuccess += (batch.Count - errors);
                    
                    // Atualizar barra de progresso final
                    task.Increment(batch.Count);
                    task.Description = $"[cyan1]Processando CSV[/] [grey]({totalSuccess} ✓ | {totalErrors} ✗)[/]";
                }
                
                task.StopTask();
            });

        // Finalizar métricas
        _metricsService.EndProcessing();
        AnsiConsole.WriteLine();
        
        // Exibir total de linhas filtradas
        if (totalFiltered > 0)
        {
            AnsiConsole.MarkupLine($"[yellow]🔍 Total de linhas filtradas:[/] [grey]{totalFiltered}[/]");
        }

        // Salvar checkpoint final
        if (!string.IsNullOrWhiteSpace(executionPaths.CheckpointPath))
        {
            await _checkpointService.SaveCheckpointAsync(
                executionPaths.CheckpointPath, 
                lineNumber, 
                totalProcessed, 
                totalSuccess, 
                totalErrors);
            
            AnsiConsole.MarkupLine($"[cyan1]💾 Checkpoint salvo em:[/] [grey]{executionPaths.CheckpointPath}[/]");
        }

        AnsiConsole.WriteLine();
        
        // Exibir dashboard final
        _metricsService.DisplayDashboard();
    }

    /// <summary>
    /// Conta o número de linhas no arquivo CSV (excluindo cabeçalho)
    /// </summary>
    private int CountCsvLines(string filePath)
    {
        try
        {
            using var reader = new StreamReader(filePath);
            int count = 0;
            while (reader.ReadLine() != null)
            {
                count++;
            }
            return count - 1; // Excluir cabeçalho
        }
        catch
        {
            return 0;
        }
    }
}

