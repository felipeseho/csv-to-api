namespace n2n.Models;

/// <summary>
///     Configuração de filtro de dados
/// </summary>
public class FilterConfiguration
{
    /// <summary>
    ///     Campo a ser filtrado
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    ///     Operador (Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, etc)
    /// </summary>
    public string Operator { get; set; } = "Equals";

    /// <summary>
    ///     Valor de comparação
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    ///     Se o filtro é case-sensitive
    /// </summary>
    public bool CaseSensitive { get; set; } = false;
}
