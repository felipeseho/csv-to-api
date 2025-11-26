using System.ComponentModel;
using Spectre.Console.Cli;

namespace n2n.Commands;

/// <summary>
///     Configurações do comando de execução de pipeline
/// </summary>
public class PipelineCommandSettings : CommandSettings
{
    [Description("Caminho para o arquivo de configuração YAML")]
    [CommandArgument(0, "<config>")]
    public string ConfigFile { get; set; } = string.Empty;

    [Description("Modo dry-run (não executa, apenas simula)")]
    [CommandOption("--dry-run")]
    public bool DryRun { get; set; }

    [Description("Modo verboso (log detalhado)")]
    [CommandOption("-v|--verbose")]
    public bool Verbose { get; set; }

    [Description("ID de execução para retomar checkpoint")]
    [CommandOption("--execution-id")]
    public string? ExecutionId { get; set; }
}
