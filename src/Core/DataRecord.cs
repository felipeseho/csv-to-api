namespace n2n.Core;

/// <summary>
///     Representa um registro de dados genérico
/// </summary>
public class DataRecord
{
    /// <summary>
    ///     Identificador único do registro (pode ser linha, ID, etc)
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    ///     Dados do registro em formato chave-valor
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    ///     Metadados adicionais do registro
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    ///     Timestamp de quando o registro foi lido/criado
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
