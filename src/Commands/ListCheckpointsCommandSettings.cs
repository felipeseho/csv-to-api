using System.ComponentModel;
using Spectre.Console.Cli;

namespace n2n.Commands;

/// <summary>
///     Configurações do comando de listagem de checkpoints
/// </summary>
public class ListCheckpointsCommandSettings : CommandSettings
{
    [Description("Diretório de checkpoints")]
    [CommandOption("-d|--directory")]
    [DefaultValue("checkpoints")]
    public string Directory { get; set; } = "checkpoints";
}
