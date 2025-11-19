using CsvToApi.Models;
using CsvToApi.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace CsvToApi;

/// <summary>
/// Classe principal da aplicação
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Criar aplicação CLI com Spectre.Console
        var app = new CommandApp<ProcessCommand>();
        app.Configure(config =>
        {
            config.SetApplicationName("csv-to-api");
            config.ValidateExamples();
        });

        try
        {
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return 1;
        }
    }
}

/// <summary>
/// Comando principal de processamento
/// </summary>
public class ProcessCommand : AsyncCommand<ProcessCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-c|--config")]
        [Description("Caminho do arquivo de configuração YAML")]
        [DefaultValue("config.yaml")]
        public string ConfigPath { get; set; } = "config.yaml";

        [CommandOption("-i|--input")]
        [Description("Caminho do arquivo CSV de entrada (sobrescreve config)")]
        public string? InputPath { get; set; }

        [CommandOption("-b|--batch-lines")]
        [Description("Número de linhas por lote (sobrescreve config)")]
        public int? BatchLines { get; set; }

        [CommandOption("-l|--log-dir")]
        [Description("Diretório onde os logs serão salvos (sobrescreve config)")]
        public string? LogDirectory { get; set; }

        [CommandOption("-d|--delimiter")]
        [Description("Delimitador do CSV (sobrescreve config)")]
        public string? Delimiter { get; set; }

        [CommandOption("-s|--start-line")]
        [Description("Linha inicial para começar o processamento (sobrescreve config)")]
        public int? StartLine { get; set; }

        [CommandOption("-n|--max-lines")]
        [Description("Número máximo de linhas a processar (sobrescreve config)")]
        public int? MaxLines { get; set; }

        [CommandOption("--exec-id|--execution-id")]
        [Description("UUID da execução para continuar de um checkpoint existente")]
        public string? ExecutionId { get; set; }

        [CommandOption("-e|--endpoint")]
        [Description("URL do endpoint da API (sobrescreve config)")]
        public string? Endpoint { get; set; }

        [CommandOption("-a|--auth-token")]
        [Description("Token de autenticação Bearer (sobrescreve config)")]
        public string? AuthToken { get; set; }

        [CommandOption("-m|--method")]
        [Description("Método HTTP: POST ou PUT (sobrescreve config)")]
        public string? Method { get; set; }

        [CommandOption("-t|--timeout")]
        [Description("Timeout das requisições em segundos (sobrescreve config)")]
        public int? Timeout { get; set; }

        [CommandOption("-v|--verbose")]
        [Description("Exibir logs detalhados")]
        [DefaultValue(false)]
        public bool Verbose { get; set; }

        [CommandOption("--dry-run|--test")]
        [Description("Modo de teste: não faz requisições reais")]
        [DefaultValue(false)]
        public bool DryRun { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            // Exibir banner
            AnsiConsole.Write(
                new FigletText("CSV to API")
                    .Centered()
                    .Color(Color.Cyan1));

            AnsiConsole.WriteLine();
            
            // Verificar se o arquivo de configuração existe
            if (!File.Exists(settings.ConfigPath))
            {
                AnsiConsole.MarkupLine($"[red]✗[/] Arquivo de configuração não encontrado: [yellow]{settings.ConfigPath}[/]");
                AnsiConsole.MarkupLine("[grey]💡 Use: csv-to-api --config caminho/do/arquivo.yaml[/]");
                return 1;
            }

            // Gerar ou usar executionId existente
            var currentExecutionId = settings.ExecutionId ?? Guid.NewGuid().ToString();
            
            // Criar opções de linha de comando
            var cmdOptions = new CommandLineOptions
            {
                ConfigPath = settings.ConfigPath,
                InputPath = settings.InputPath,
                BatchLines = settings.BatchLines,
                LogDirectory = settings.LogDirectory,
                CsvDelimiter = settings.Delimiter,
                StartLine = settings.StartLine,
                MaxLines = settings.MaxLines,
                ExecutionId = currentExecutionId,
                EndpointUrl = settings.Endpoint,
                AuthToken = settings.AuthToken,
                Method = settings.Method,
                RequestTimeout = settings.Timeout,
                Verbose = settings.Verbose,
                DryRun = settings.DryRun
            };

            // Mostrar configuração se verbose
            if (settings.Verbose)
            {
                var configTable = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Grey)
                    .AddColumn(new TableColumn("[cyan1]Configuração[/]").Centered())
                    .AddColumn(new TableColumn("[cyan1]Valor[/]"));

                configTable.AddRow("Config", settings.ConfigPath);
                if (settings.InputPath != null) configTable.AddRow("Input", settings.InputPath);
                if (settings.BatchLines != null) configTable.AddRow("Batch Lines", settings.BatchLines.ToString()!);
                if (settings.StartLine != null) configTable.AddRow("Start Line", settings.StartLine.ToString()!);
                if (settings.MaxLines != null) configTable.AddRow("Max Lines", settings.MaxLines.ToString()!);
                if (settings.Endpoint != null) configTable.AddRow("Endpoint", settings.Endpoint);
                if (settings.DryRun) configTable.AddRow("[yellow]Modo[/]", "[yellow]DRY RUN[/]");

                AnsiConsole.Write(configTable);
                AnsiConsole.WriteLine();
            }

            // Inicializar serviços
            var configService = new ConfigurationService();
            var validationService = new ValidationService();
            var loggingService = new LoggingService();
            var checkpointService = new CheckpointService();
            var metricsService = new MetricsService();

            // Carregar configuração do YAML
            Configuration config;
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan1"))
                .StartAsync("[cyan1]Carregando configuração...[/]", async ctx =>
                {
                    await Task.Run(() =>
                    {
                        config = configService.LoadConfiguration(settings.ConfigPath);
                    });
                });

            config = configService.LoadConfiguration(settings.ConfigPath);

            // Mesclar com opções de linha de comando
            config = configService.MergeWithCommandLineOptions(config, cmdOptions);

            // Validar configuração final
            if (!configService.ValidateConfiguration(config))
            {
                AnsiConsole.MarkupLine("[red]✗ Configuração inválida[/]");
                return 1;
            }

            // Criar diretórios necessários
            configService.EnsureDirectoriesExist(config);

            // Exibir UUID da execução
            var panel = new Panel(
                new Markup(settings.ExecutionId != null 
                    ? $"[cyan1]🔄 Continuando execução[/]\n[yellow]{currentExecutionId}[/]"
                    : $"[cyan1]✨ Nova execução iniciada[/]\n[yellow]{currentExecutionId}[/]"))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Header("[cyan1]Execution ID[/]");

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            // Gerar caminhos de execução
            var executionPaths = configService.GenerateExecutionPaths(config, currentExecutionId);

            // Inicializar ApiClientService com a configuração da API e MetricsService
            var apiClientService = new ApiClientService(loggingService, config.Api, metricsService);
            var processorService = new CsvProcessorService(validationService, loggingService, apiClientService, checkpointService, metricsService);

            if (settings.DryRun)
            {
                AnsiConsole.MarkupLine("[yellow]🔍 MODO DRY RUN: Nenhuma requisição será enviada à API[/]");
                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine("[cyan1]🚀 Iniciando processamento do arquivo CSV...[/]");
            AnsiConsole.WriteLine();

            // Processar arquivo CSV
            await processorService.ProcessCsvFileAsync(config, executionPaths, settings.DryRun);

            // Sucesso
            var successRule = new Rule("[green]✓ Processamento concluído com sucesso![/]")
                .RuleStyle(Style.Parse("green"));
            AnsiConsole.Write(successRule);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[red]✗ Erro durante o processamento[/]");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes);
            return 1;
        }
    }
}





