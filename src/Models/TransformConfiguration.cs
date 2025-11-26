namespace n2n.Models;

/// <summary>
///     Configuração de transformação de dados
/// </summary>
public class TransformConfiguration
{
    /// <summary>
    ///     Campo a ser transformado
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    ///     Tipo de transformação (ToUpper, ToLower, Trim, Format, Custom, etc)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Parâmetros da transformação
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}
