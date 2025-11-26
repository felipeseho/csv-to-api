namespace n2n.Core;

/// <summary>
///     Métricas específicas da origem de dados
/// </summary>
public class SourceMetrics
{
    public long TotalRecordsRead { get; set; }
    public long FilteredRecords { get; set; }
    public long ValidationErrors { get; set; }
    public long BytesRead { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public double RecordsPerSecond { get; set; }
    
    /// <summary>
    ///     Métricas customizadas específicas da origem
    /// </summary>
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
}
