namespace n2n.Models;

/// <summary>
///     Configuração de origem de dados
/// </summary>
public class DataSourceConfiguration
{
    /// <summary>
    ///     Tipo de origem (CSV, Database, Queue, API, etc)
    /// </summary>
    public string Type { get; set; } = "CSV";

    /// <summary>
    ///     Configurações específicas da origem
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}
