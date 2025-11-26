namespace n2n.Models;

/// <summary>
///     Configuração de destino de dados
/// </summary>
public class DataDestinationConfiguration
{
    /// <summary>
    ///     Tipo de destino (API, Database, Queue, File, etc)
    /// </summary>
    public string Type { get; set; } = "API";

    /// <summary>
    ///     Configurações específicas do destino
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}
