namespace CsvToApi.Models;

/// <summary>
/// Configuração da API REST
/// </summary>
public class ApiConfiguration
{
    public string EndpointUrl { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public int RequestTimeout { get; set; } = 30;
    public List<ApiMapping> Mapping { get; set; } = new();
}

