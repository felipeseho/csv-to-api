using n2n.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace n2n.Commands;

/// <summary>
///     Comando para listar checkpoints disponíveis
/// </summary>
public class ListCheckpointsCommand : Command<ListCheckpointsCommandSettings>
{
    private readonly PipelineCheckpointService _checkpointService;

    public ListCheckpointsCommand(PipelineCheckpointService checkpointService)
    {
        _checkpointService = checkpointService;
    }

    public override int Execute(CommandContext context, ListCheckpointsCommandSettings settings, CancellationToken cancellationToken)
    {
        var checkpoints = _checkpointService.ListCheckpoints(settings.Directory);

        if (checkpoints.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nenhum checkpoint encontrado[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn("[cyan1]Execution ID[/]")
            .AddColumn("[cyan1]Pipeline[/]")
            .AddColumn("[cyan1]Origem[/]")
            .AddColumn("[cyan1]Destino[/]")
            .AddColumn("[cyan1]Processados[/]")
            .AddColumn("[cyan1]Sucessos[/]")
            .AddColumn("[cyan1]Erros[/]")
            .AddColumn("[cyan1]Atualizado[/]");

        foreach (var checkpoint in checkpoints)
        {
            var executionId = checkpoint.ExecutionId.Length > 12 
                ? checkpoint.ExecutionId.Substring(0, 12) + "..." 
                : checkpoint.ExecutionId;

            var successRate = checkpoint.TotalProcessed > 0
                ? (checkpoint.SuccessCount * 100.0 / checkpoint.TotalProcessed)
                : 0;

            var successColor = successRate >= 95 ? "green" : successRate >= 80 ? "yellow" : "red";

            table.AddRow(
                $"[yellow]{executionId}[/]",
                checkpoint.PipelineName,
                $"[blue]{checkpoint.SourceType}[/]",
                $"[green]{checkpoint.DestinationType}[/]",
                $"{checkpoint.TotalProcessed:N0}",
                $"[{successColor}]{checkpoint.SuccessCount:N0}[/]",
                $"[red]{checkpoint.ErrorCount:N0}[/]",
                $"[grey]{FormatDateTime(checkpoint.UpdatedAt)}[/]"
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[cyan1]Total:[/] {checkpoints.Count} checkpoint(s)");
        AnsiConsole.MarkupLine($"[grey]Para retomar uma execução, use:[/] [yellow]n2n <config-file> --execution-id <id>[/]");

        return 0;
    }

    private string FormatDateTime(DateTime dateTime)
    {
        var local = dateTime.ToLocalTime();
        var elapsed = DateTime.Now - local;

        if (elapsed.TotalHours < 24)
        {
            return $"{(int)elapsed.TotalHours}h atrás";
        }
        if (elapsed.TotalDays < 7)
        {
            return $"{(int)elapsed.TotalDays}d atrás";
        }
        return local.ToString("dd/MM/yyyy HH:mm");
    }
}
