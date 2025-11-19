namespace CsvToApi.Models;

/// <summary>
/// Mapeamento entre coluna CSV e atributo da API
/// </summary>
public class ApiMapping
{
    public string Attribute { get; set; } = string.Empty;
    public string CsvColumn { get; set; } = string.Empty;
    public string? Transform { get; set; }
}

