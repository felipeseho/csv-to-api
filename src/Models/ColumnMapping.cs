namespace n2n.Models;

/// <summary>
///     Mapeamento e validação de coluna CSV
/// </summary>
public class ColumnMapping
{
    public string Column { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Regex { get; set; }
    public string? Format { get; set; }

    /// <summary>
    ///     Filtro opcional a ser aplicado nesta coluna (para compatibilidade com configurações antigas)
    /// </summary>
    public ColumnFilter? Filter { get; set; }

    /// <summary>
    ///     Lista de filtros a serem aplicados nesta coluna (modo recomendado para múltiplos filtros)
    /// </summary>
    public List<ColumnFilter>? Filters { get; set; }
}