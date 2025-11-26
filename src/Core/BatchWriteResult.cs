namespace n2n.Core;

/// <summary>
///     Resultado de escrita de lote de registros
/// </summary>
public class BatchWriteResult
{
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public List<WriteResult> IndividualResults { get; set; } = new();
    public long TotalResponseTimeMs { get; set; }

    public double SuccessRate => TotalRecords > 0 ? (SuccessCount * 100.0 / TotalRecords) : 0;
}
