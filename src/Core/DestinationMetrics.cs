namespace n2n.Core;

/// <summary>
///     Métricas específicas do destino de dados
/// </summary>
public class DestinationMetrics
{
    public long TotalRecordsWritten { get; set; }
    public long SuccessCount { get; set; }
    public long ErrorCount { get; set; }
    public long TotalRetries { get; set; }
    public long BytesWritten { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public double RecordsPerSecond { get; set; }
    public long AverageResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    
    /// <summary>
    ///     Métricas customizadas específicas do destino
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}
