namespace n2n.Models;

/// <summary>
///     Configuração de pipeline de dados
/// </summary>
public class PipelineConfiguration
{
    /// <summary>
    ///     Nome do pipeline
    /// </summary>
    public string Name { get; set; } = "default";

    /// <summary>
    ///     Descrição do pipeline
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Configuração da origem de dados
    /// </summary>
    public DataSourceConfiguration Source { get; set; } = new();

    /// <summary>
    ///     Configuração do destino de dados
    /// </summary>
    public DataDestinationConfiguration Destination { get; set; } = new();

    /// <summary>
    ///     Configurações de processamento
    /// </summary>
    public ProcessingConfiguration Processing { get; set; } = new();

    /// <summary>
    ///     Transformações a serem aplicadas
    /// </summary>
    public List<TransformConfiguration> Transforms { get; set; } = new();

    /// <summary>
    ///     Filtros a serem aplicados
    /// </summary>
    public List<FilterConfiguration> Filters { get; set; } = new();
}
