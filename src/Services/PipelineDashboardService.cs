using System.Reflection;
using n2n.Core;
using n2n.Models;
using Spectre.Console;

namespace n2n.Services;

/// <summary>
///     Serviço de dashboard para múltiplas origens e destinos
/// </summary>
public class PipelineDashboardService
{
    private readonly List<string> _logMessages = new();
    private readonly object _logMessagesLock = new();
    private readonly int _maxLogMessages = 10;
    private readonly string _appName = Assembly.GetExecutingAssembly().GetName().Name!;
    private readonly string _appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString()!;
    private readonly PipelineConfiguration _configuration;
    private IDataSource? _source;
    private long? _estimatedTotal;
    private IDataDestination? _destination;
    private bool _isRunning;

    public PipelineDashboardService(PipelineConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void SetSource(IDataSource source) => _source = source;
    public void SetDestination(IDataDestination destination) => _destination = destination;
    public void SetEstimatedTotal(long? total) => _estimatedTotal = total;

    public void AddLogMessage(string message, string level = "INFO")
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var color = level.ToUpper() switch
        {
            "ERROR" => "red",
            "WARNING" => "yellow",
            "SUCCESS" => "green",
            "INFO" => "cyan1",
            _ => "grey"
        };

        var levelIcon = level.ToUpper() switch
        {
            "ERROR" => "❌",
            "WARNING" => "⚠️",
            "SUCCESS" => "✅",
            "INFO" => "ℹ️ ",
            _ => "."
        };

        var formattedMessage = $"[grey]{timestamp}[/] [{color}]{levelIcon}[/] {message}";
        
        lock (_logMessagesLock)
        {
            _logMessages.Add(formattedMessage);

            if (_logMessages.Count > _maxLogMessages)
            {
                _logMessages.RemoveAt(0);
            }
        }
    }

    public async Task StartLiveDashboard(CancellationToken cancellationToken)
    {
        _isRunning = true;

        try
        {
            if (!ValidateTerminalSize())
            {
                AnsiConsole.MarkupLine(
                    "[yellow]⚠️  Terminal pequeno. Use terminal de no mínimo 80x25 para dashboard completo.[/]");
                AnsiConsole.WriteLine();

                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        ShowSimpleSummary();
                        await Task.Delay(2000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                return;
            }

            AnsiConsole.Clear();

            await AnsiConsole.Live(CreateLayout())
                .AutoClear(false)
                .Overflow(VerticalOverflow.Visible)
                .Cropping(VerticalOverflowCropping.Bottom)
                .StartAsync(async ctx =>
                {
                    while (_isRunning && !cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            ctx.UpdateTarget(CreateLayout());
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Terminal redimensionado
                        }

                        await Task.Delay(500, cancellationToken);
                    }
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Erro ao iniciar dashboard: {ex.Message}[/]");

            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public void Stop() => _isRunning = false;

    public void UpdateOnce()
    {
        try
        {
            if (!ValidateTerminalSize())
            {
                ShowSimpleSummary();
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(CreateLayout());
        }
        catch
        {
            ShowSimpleSummary();
        }
    }

    private bool ValidateTerminalSize()
    {
        try
        {
            return Console.WindowWidth >= 80 && Console.WindowHeight >= 25;
        }
        catch
        {
            return false;
        }
    }

    private void ShowSimpleSummary()
    {
        var sourceMetrics = _source?.GetMetrics();
        var destMetrics = _destination?.GetMetrics();

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[cyan1]═══ Pipeline: {_configuration.Name} ═══[/]");

        if (sourceMetrics != null)
        {
            AnsiConsole.MarkupLine(
                $"[cyan1]Origem ({_configuration.Source.Type}):[/] {sourceMetrics.TotalRecordsRead:N0} lidos");
        }

        if (destMetrics != null)
        {
            AnsiConsole.MarkupLine(
                $"[cyan1]Destino ({_configuration.Destination.Type}):[/] {destMetrics.TotalRecordsWritten:N0} escritos");
            AnsiConsole.MarkupLine(
                $"[green]✅ Sucessos:[/] {destMetrics.SuccessCount:N0} | [red]❌ Erros:[/] {destMetrics.ErrorCount:N0}");
        }

        lock (_logMessagesLock)
        {
            if (_logMessages.Count > 0)
            {
                AnsiConsole.MarkupLine($"[grey]Último log:[/] {_logMessages.Last()}");
            }
        }

        AnsiConsole.WriteLine();
    }

    private Layout CreateLayout()
    {
        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(5),
                new Layout("Body"),
                new Layout("Footer").Size(13)
            );

        layout["Header"].Update(CreateHeaderPanel());

        layout["Body"].SplitRows(
            new Layout("Row1"),
            new Layout("Row2")
        );

        layout["Body"]["Row1"].SplitColumns(
            new Layout("Source").Ratio(1),
            new Layout("Processing").Ratio(1)
        );

        layout["Body"]["Row1"]["Source"].Update(CreateSourcePanel());
        layout["Body"]["Row1"]["Processing"].Update(CreateProcessingPanel());

        layout["Body"]["Row2"].SplitColumns(
            new Layout("Destination").Ratio(1),
            new Layout("Progress").Ratio(1)
        );

        layout["Body"]["Row2"]["Destination"].Update(CreateDestinationPanel());
        layout["Body"]["Row2"]["Progress"].Update(CreateProgressPanel(_estimatedTotal));

        layout["Footer"].Update(CreateLogsPanel());

        return layout;
    }

    private Panel CreateHeaderPanel()
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().LeftAligned());

        grid.AddRow(new Markup($"[bold cyan1]{_appName}[/] - Pipeline: [yellow]{_configuration.Name}[/]"));
        if (!string.IsNullOrWhiteSpace(_configuration.Description))
        {
            grid.AddRow(new Markup($"[grey]{_configuration.Description}[/]"));
        }

        grid.AddRow(new Markup($"[grey]Versão {_appVersion}[/]"));

        return new Panel(grid)
            .BorderColor(Color.Cyan1)
            .Padding(0, 0)
            .Expand();
    }

    private Panel CreateSourcePanel()
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[cyan1]Tipo:[/]", $"[yellow]{_configuration.Source.Type}[/]");

        var sourceMetrics = _source?.GetMetrics();
        if (sourceMetrics != null)
        {
            grid.AddRow("[cyan1]Lidos:[/]", $"[yellow]{sourceMetrics.TotalRecordsRead:N0}[/]");
            grid.AddRow("[cyan1]Filtrados:[/]", $"[grey]{sourceMetrics.FilteredRecords:N0}[/]");
            grid.AddRow("[cyan1]Tempo:[/]", $"[yellow]{FormatTimeSpan(sourceMetrics.ElapsedTime)}[/]");
            grid.AddRow("[cyan1]Velocidade:[/]", $"[green]{sourceMetrics.RecordsPerSecond:F1}[/] rec/s");

            // Métricas customizadas
            if (sourceMetrics.CustomMetrics.Count > 0)
            {
                grid.AddEmptyRow();
                grid.AddRow(new Markup("[underline cyan1]Métricas:[/]"), new Markup(""));
                foreach (var metric in sourceMetrics.CustomMetrics.Take(3))
                {
                    grid.AddRow($"  {metric.Key}:", $"[grey]{metric.Value}[/]");
                }
            }
        }

        // Configurações da origem
        grid.AddEmptyRow();
        grid.AddRow(new Markup("[underline cyan1]Configuração:[/]"), new Markup(""));
        foreach (var setting in _configuration.Source.Settings.Take(5))
        {
            var value = setting.Value?.ToString() ?? "null";
            if (value.Length > 30) value = value.Substring(0, 27) + "...";
            grid.AddRow($"  {setting.Key}:", $"[grey]{value}[/]");
        }

        return new Panel(grid)
            .Header($"[bold cyan1]📥 ORIGEM[/]", Justify.Center)
            .BorderColor(Color.Blue)
            .Padding(0, 0)
            .Expand();
    }

    private Panel CreateDestinationPanel()
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[cyan1]Tipo:[/]", $"[yellow]{_configuration.Destination.Type}[/]");

        var destMetrics = _destination?.GetMetrics();
        if (destMetrics != null)
        {
            grid.AddRow("[cyan1]Escritos:[/]", $"[yellow]{destMetrics.TotalRecordsWritten:N0}[/]");
            grid.AddRow("[green]✅ Sucessos:[/]", $"[green]{destMetrics.SuccessCount:N0}[/]");
            grid.AddRow("[red]❌ Erros:[/]", $"[red]{destMetrics.ErrorCount:N0}[/]");
            grid.AddRow("[cyan1]Tentativas:[/]", $"[yellow]{destMetrics.TotalRetries}[/]");
            grid.AddEmptyRow();
            grid.AddRow("[cyan1]Resp. Média:[/]", $"[yellow]{destMetrics.AverageResponseTimeMs}[/] ms");

            if (destMetrics.MinResponseTimeMs != long.MaxValue)
            {
                grid.AddRow("[cyan1]Mín / Máx:[/]",
                    $"[green]{destMetrics.MinResponseTimeMs}[/] / [red]{destMetrics.MaxResponseTimeMs}[/] ms");
            }

            // Status codes se disponível
            if (destMetrics.CustomMetrics.TryGetValue("StatusCodes", out var statusCodesObj) &&
                statusCodesObj is Dictionary<int, long> statusCodes)
            {
                grid.AddEmptyRow();
                var statusList = new List<string>();
                foreach (var kvp in statusCodes.OrderBy(x => x.Key).Take(4))
                {
                    var color = GetStatusColor(kvp.Key);
                    statusList.Add($"[{color}]{kvp.Key}[/]:[{color}]{kvp.Value}[/]");
                }

                var statusLine = string.Join(" ", statusList);
                if (statusCodes.Count > 4)
                    statusLine += $" [grey]+{statusCodes.Count - 4}[/]";

                grid.AddRow(new Markup($"[grey]HTTP:[/] {statusLine}"));
            }
        }

        return new Panel(grid)
            .Header($"[bold cyan1]📤 DESTINO[/]", Justify.Center)
            .BorderColor(Color.Green)
            .Padding(0, 0)
            .Expand();
    }

    private Panel CreateProcessingPanel()
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn();

        grid.AddRow("[cyan1]Logs:[/]", $"[grey]{_configuration.Processing.LogDirectory}[/]");
        grid.AddRow("[cyan1]Checkpoint:[/]", $"[grey]{_configuration.Processing.CheckpointDirectory}[/]");

        if (_configuration.Filters.Count > 0)
        {
            grid.AddEmptyRow();
            grid.AddRow("[cyan1]Filtros:[/]", $"[blue]{_configuration.Filters.Count} ativo(s)[/]");
        }

        if (_configuration.Transforms.Count > 0)
        {
            grid.AddRow("[cyan1]Transformações:[/]", $"[blue]{_configuration.Transforms.Count} ativa(s)[/]");
        }

        return new Panel(grid)
            .Header("[bold cyan1]⚙️ PROCESSAMENTO[/]", Justify.Center)
            .BorderColor(Color.Cyan1)
            .Padding(0, 0)
            .Expand();
    }

    private Panel CreateProgressPanel(long? estimatedTotal)
    {
        var grid = new Grid().AddColumn();

        var sourceMetrics = _source?.GetMetrics();
        var destMetrics = _destination?.GetMetrics();

        if (sourceMetrics != null && destMetrics != null)
        {
            var processed = destMetrics.TotalRecordsWritten;
            var total = estimatedTotal ?? sourceMetrics.TotalRecordsRead;
            var percentage = total > 0 ? (processed * 100.0 / total) : 0;

            var barWidth = 35;
            var filledWidth = Math.Max(0, Math.Min(barWidth, (int)(barWidth * percentage / 100)));
            var emptyWidth = Math.Max(0, barWidth - filledWidth);

            var filledBar = filledWidth > 0 ? new string('█', filledWidth) : "";
            var emptyBar = emptyWidth > 0 ? new string('░', emptyWidth) : "";
            var bar = $"[green]{filledBar}[/][grey]{emptyBar}[/]";

            grid.AddRow(new Markup($"{bar} [yellow]{percentage:F1}%[/]").Centered());
            grid.AddEmptyRow();

            var statsGrid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(2))
                .AddColumn();

            statsGrid.AddRow("[cyan1]Processados:[/]", $"[yellow]{processed:N0}[/] / [grey]{total:N0}[/]");

            var successRate = destMetrics.TotalRecordsWritten > 0
                ? destMetrics.SuccessCount * 100.0 / destMetrics.TotalRecordsWritten
                : 0;
            var errorRate = destMetrics.TotalRecordsWritten > 0
                ? destMetrics.ErrorCount * 100.0 / destMetrics.TotalRecordsWritten
                : 0;

            statsGrid.AddRow("[green]✅ Sucessos:[/]",
                $"[green]{destMetrics.SuccessCount:N0}[/] [grey]({successRate:F1}%)[/]");
            statsGrid.AddRow("[red]❌ Erros:[/]",
                $"[red]{destMetrics.ErrorCount:N0}[/] [grey]({errorRate:F1}%)[/]");

            grid.AddRow(statsGrid);
            grid.AddEmptyRow();

            var timeGrid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(2))
                .AddColumn();

            timeGrid.AddRow("[cyan1]⏱️  Decorrido:[/]", $"[yellow]{FormatTimeSpan(destMetrics.ElapsedTime)}[/]");
            timeGrid.AddRow("[cyan1]🚀 Velocidade:[/]",
                $"[green]{destMetrics.RecordsPerSecond:F1}[/] [grey]rec/s[/]");

            grid.AddRow(timeGrid);
        }
        else
        {
            grid.AddRow(new Markup("[grey]Aguardando dados...[/]"));
        }

        return new Panel(grid)
            .Header("[bold cyan1]📊 PROGRESSO[/]", Justify.Center)
            .BorderColor(Color.Purple)
            .Padding(0, 0)
            .Expand();
    }

    private Panel CreateLogsPanel()
    {
        var grid = new Grid().AddColumn();

        lock (_logMessagesLock)
        {
            if (_logMessages.Count == 0)
            {
                grid.AddRow(new Markup("[grey]Aguardando eventos...[/]"));
            }
            else
            {
                foreach (var log in _logMessages.ToArray())
                {
                    grid.AddRow(new Markup(log));
                }
            }
        }

        return new Panel(grid)
            .Header("[bold cyan1]📋 LOGS DE EXECUÇÃO[/]", Justify.Center)
            .BorderColor(Color.Orange1)
            .Padding(0, 0)
            .Expand();
    }

    private string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalSeconds < 60)
            return $"{ts.TotalSeconds:F0}s";
        if (ts.TotalMinutes < 60)
            return $"{(int)ts.TotalMinutes}min {ts.Seconds}s";
        return $"{(int)ts.TotalHours}h {ts.Minutes}min";
    }

    private string GetStatusColor(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => "green",
            >= 300 and < 400 => "blue",
            >= 400 and < 500 => "yellow",
            >= 500 => "red",
            _ => "grey"
        };
    }
}
