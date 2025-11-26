using n2n.Factories;
using n2n.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace n2n.Commands;

/// <summary>
///     Comando principal para execução de pipeline de dados
/// </summary>
public class PipelineCommand : AsyncCommand<PipelineCommandSettings>
{
    private readonly IDataSourceFactory _sourceFactory;
    private readonly IDataDestinationFactory _destinationFactory;
    private readonly PipelineConfigurationService _configService;
    private readonly PipelineCheckpointService _checkpointService;

    public PipelineCommand(
        IDataSourceFactory sourceFactory,
        IDataDestinationFactory destinationFactory,
        PipelineConfigurationService configService,
        PipelineCheckpointService checkpointService)
    {
        _sourceFactory = sourceFactory;
        _destinationFactory = destinationFactory;
        _configService = configService;
        _checkpointService = checkpointService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, PipelineCommandSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            // Banner
            DisplayBanner();

            // Carregar configuração
            AnsiConsole.MarkupLine($"[cyan1]📄 Carregando configuração:[/] [yellow]{settings.ConfigFile}[/]");
            var configuration = _configService.LoadFromFile(settings.ConfigFile);

            // Validar configuração
            var errors = _configService.ValidateConfiguration(configuration);
            if (errors.Count > 0)
            {
                AnsiConsole.MarkupLine("[red]❌ Erros na configuração:[/]");
                foreach (var error in errors)
                {
                    AnsiConsole.MarkupLine($"  [red]- {error}[/]");
                }

                return 1;
            }

            AnsiConsole.MarkupLine($"[green]✅ Configuração válida: Pipeline '{configuration.Name}'[/]");
            AnsiConsole.WriteLine();

            // Exibir informações do pipeline
            DisplayPipelineInfo(configuration);

            // Determinar execution ID
            string executionId;
            if (!string.IsNullOrWhiteSpace(settings.ExecutionId))
            {
                executionId = settings.ExecutionId;
                AnsiConsole.MarkupLine($"[cyan1]🔄 Retomando execução:[/] [yellow]{executionId}[/]");
                
                // Verificar se checkpoint existe
                var existingCheckpoint = await _checkpointService.LoadCheckpointAsync(
                    configuration.Processing.CheckpointDirectory,
                    executionId);
                
                if (existingCheckpoint == null)
                {
                    AnsiConsole.MarkupLine("[yellow]⚠️  Checkpoint não encontrado. Iniciando nova execução.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[green]✅ Checkpoint encontrado:[/] {existingCheckpoint.TotalProcessed} registros processados");
                }
            }
            else
            {
                executionId = _checkpointService.GenerateExecutionId();
                AnsiConsole.MarkupLine($"[cyan1]🆕 Nova execução:[/] [yellow]{executionId}[/]");
            }
            
            AnsiConsole.WriteLine();

            if (settings.DryRun)
            {
                AnsiConsole.MarkupLine("[yellow]⚠️  Modo DRY RUN - Nenhuma operação será executada[/]");
                AnsiConsole.WriteLine();
                return 0;
            }

            // Criar serviços
            var dashboardService = new PipelineDashboardService(configuration);

            // Criar e executar pipeline
            var pipelineService = new DataPipelineService(
                _sourceFactory,
                _destinationFactory,
                configuration,
                dashboardService,
                _checkpointService,
                executionId
            );

            await pipelineService.ExecuteAsync();

            AnsiConsole.MarkupLine("[green]✅ Pipeline executado com sucesso![/]");
            return 0;
        }
        catch (FileNotFoundException ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ {ex.Message}[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Erro: {ex.Message}[/]");
            if (settings.Verbose)
            {
                AnsiConsole.WriteException(ex);
            }

            return 1;
        }
    }

    private void DisplayBanner()
    {
        AnsiConsole.Write(new FigletText("N2N")
            .Centered()
            .Color(Color.Cyan1));

        AnsiConsole.MarkupLine("[grey]Node to Node - Pipeline de Dados Universal[/]");
        AnsiConsole.WriteLine();
    }

    private void DisplayPipelineInfo(Models.PipelineConfiguration configuration)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn("[cyan1]Item[/]")
            .AddColumn("[yellow]Valor[/]");

        table.AddRow("Pipeline", configuration.Name);
        if (!string.IsNullOrWhiteSpace(configuration.Description))
        {
            table.AddRow("Descrição", configuration.Description);
        }

        table.AddRow("Origem", $"{configuration.Source.Type}");
        table.AddRow("Destino", $"{configuration.Destination.Type}");

        if (configuration.Filters.Count > 0)
        {
            table.AddRow("Filtros", $"{configuration.Filters.Count} configurado(s)");
        }

        if (configuration.Transforms.Count > 0)
        {
            table.AddRow("Transformações", $"{configuration.Transforms.Count} configurada(s)");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
