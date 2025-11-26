namespace n2n.Models;

/// <summary>
///     Checkpoint de execução de pipeline
/// </summary>
public class PipelineCheckpoint
{
    /// <summary>
    ///     ID único da execução
    /// </summary>
    public string ExecutionId { get; set; } = string.Empty;

    /// <summary>
    ///     Nome do pipeline
    /// </summary>
    public string PipelineName { get; set; } = string.Empty;

    /// <summary>
    ///     Tipo de origem
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    ///     Tipo de destino
    /// </summary>
    public string DestinationType { get; set; } = string.Empty;

    /// <summary>
    ///     Timestamp de início
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Timestamp da última atualização
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Último ID de registro processado
    /// </summary>
    public string LastProcessedRecordId { get; set; } = string.Empty;

    /// <summary>
    ///     Total de registros processados
    /// </summary>
    public long TotalProcessed { get; set; }

    /// <summary>
    ///     Total de sucessos
    /// </summary>
    public long SuccessCount { get; set; }

    /// <summary>
    ///     Total de erros
    /// </summary>
    public long ErrorCount { get; set; }

    /// <summary>
    ///     Estado customizado específico da origem
    /// </summary>
    public Dictionary<string, object> SourceState { get; set; } = new();

    /// <summary>
    ///     Estado customizado específico do destino
    /// </summary>
    public Dictionary<string, object> DestinationState { get; set; } = new();

    /// <summary>
    ///     Metadados adicionais
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
