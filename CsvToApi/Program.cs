using CsvToApi.Models;
using CsvToApi.Services;
using System.CommandLine;

namespace CsvToApi;

/// <summary>
/// Classe principal da aplicação
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Configurar opções de linha de comando
        var rootCommand = new RootCommand("CSV to API - Processador de arquivos CSV grandes com envio para API REST");

        // Opções de configuração
        var configOption = new Option<string>(
            aliases: new[] { "--config", "-c" },
            description: "Caminho do arquivo de configuração YAML",
            getDefaultValue: () => "config.yaml");

        // Opções de arquivo CSV
        var inputOption = new Option<string?>(
            aliases: new[] { "--input", "-i" },
            description: "Caminho do arquivo CSV de entrada (sobrescreve config)");

        var batchLinesOption = new Option<int?>(
            aliases: new[] { "--batch-lines", "-b" },
            description: "Número de linhas por lote (sobrescreve config)");

        var logPathOption = new Option<string?>(
            aliases: new[] { "--log-path", "-l" },
            description: "Caminho do arquivo de log (sobrescreve config)");

        var delimiterOption = new Option<string?>(
            aliases: new[] { "--delimiter", "-d" },
            description: "Delimitador do CSV (sobrescreve config)");

        var startLineOption = new Option<int?>(
            aliases: new[] { "--start-line", "-s" },
            description: "Linha inicial para começar o processamento (sobrescreve config)");

        // Opções de API
        var endpointOption = new Option<string?>(
            aliases: new[] { "--endpoint", "-e" },
            description: "URL do endpoint da API (sobrescreve config)");

        var authTokenOption = new Option<string?>(
            aliases: new[] { "--auth-token", "-a" },
            description: "Token de autenticação Bearer (sobrescreve config)");

        var methodOption = new Option<string?>(
            aliases: new[] { "--method", "-m" },
            description: "Método HTTP: POST ou PUT (sobrescreve config)");

        var timeoutOption = new Option<int?>(
            aliases: new[] { "--timeout", "-t" },
            description: "Timeout das requisições em segundos (sobrescreve config)");

        // Opções gerais
        var verboseOption = new Option<bool>(
            aliases: new[] { "--verbose", "-v" },
            description: "Exibir logs detalhados");

        // Adicionar opções ao comando raiz
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(batchLinesOption);
        rootCommand.AddOption(logPathOption);
        rootCommand.AddOption(delimiterOption);
        rootCommand.AddOption(startLineOption);
        rootCommand.AddOption(endpointOption);
        rootCommand.AddOption(authTokenOption);
        rootCommand.AddOption(methodOption);
        rootCommand.AddOption(timeoutOption);
        rootCommand.AddOption(verboseOption);

        // Handler do comando usando binding individual (limitado a 8 parâmetros)
        rootCommand.SetHandler(
            async (configPath, inputPath, batchLines, logPath, delimiter, startLine, endpoint, verbose) =>
            {
                // Obter método e timeout do ParseResult se necessário
                var authToken = rootCommand.Parse(args).GetValueForOption(authTokenOption);
                var method = rootCommand.Parse(args).GetValueForOption(methodOption);
                var timeout = rootCommand.Parse(args).GetValueForOption(timeoutOption);
                
                await ProcessCsvAsync(configPath, inputPath, batchLines, logPath, delimiter, startLine, endpoint, authToken, method, timeout, verbose);
            },
            configOption,
            inputOption,
            batchLinesOption,
            logPathOption,
            delimiterOption,
            startLineOption,
            endpointOption,
            verboseOption);

        // Executar comando
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ProcessCsvAsync(
        string configPath,
        string? inputPath,
        int? batchLines,
        string? logPath,
        string? delimiter,
        int? startLine,
        string? endpoint,
        string? authToken,
        string? method,
        int? timeout,
        bool verbose)
    {
        try
        {
            // Verificar se o arquivo de configuração existe
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"❌ Arquivo de configuração não encontrado: {configPath}");
                Console.WriteLine("💡 Use: CsvToApi --config caminho/do/arquivo.yaml");
                Environment.Exit(1);
            }

            // Criar opções de linha de comando
            var cmdOptions = new CommandLineOptions
            {
                ConfigPath = configPath,
                InputPath = inputPath,
                BatchLines = batchLines,
                LogPath = logPath,
                CsvDelimiter = delimiter,
                StartLine = startLine,
                EndpointUrl = endpoint,
                AuthToken = authToken,
                Method = method,
                RequestTimeout = timeout,
                Verbose = verbose
            };

            if (verbose)
            {
                Console.WriteLine("📋 Configuração carregada:");
                Console.WriteLine($"  Config: {configPath}");
                if (inputPath != null) Console.WriteLine($"  Input: {inputPath}");
                if (batchLines != null) Console.WriteLine($"  Batch Lines: {batchLines}");
                if (startLine != null) Console.WriteLine($"  Start Line: {startLine}");
                if (endpoint != null) Console.WriteLine($"  Endpoint: {endpoint}");
            }

            // Inicializar serviços
            var configService = new ConfigurationService();
            var validationService = new ValidationService();
            var loggingService = new LoggingService();
            var apiClientService = new ApiClientService(loggingService);
            var processorService = new CsvProcessorService(validationService, loggingService, apiClientService);

            // Carregar configuração do YAML
            var config = configService.LoadConfiguration(configPath);

            // Mesclar com opções de linha de comando
            config = configService.MergeWithCommandLineOptions(config, cmdOptions);

            // Validar configuração final
            if (!configService.ValidateConfiguration(config))
            {
                Environment.Exit(1);
            }

            // Criar diretórios necessários
            configService.EnsureDirectoriesExist(config);

            Console.WriteLine("🚀 Iniciando processamento do arquivo CSV...");

            // Processar arquivo CSV
            await processorService.ProcessCsvFileAsync(config);

            Console.WriteLine("✅ Processamento concluído com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro durante o processamento: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            Environment.Exit(1);
        }
    }
}





