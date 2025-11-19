namespace CsvToApi.Models;

/// <summary>
/// Caminhos de arquivos gerados para uma execução específica
/// </summary>
public class ExecutionPaths
{
    public string LogPath { get; set; } = string.Empty;
    public string CheckpointPath { get; set; } = string.Empty;
}
