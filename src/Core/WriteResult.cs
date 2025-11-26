namespace n2n.Core;

/// <summary>
///     Resultado de escrita de um único registro
/// </summary>
public class WriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public static WriteResult SuccessResult(long responseTimeMs = 0) => new()
    {
        Success = true,
        ResponseTimeMs = responseTimeMs
    };

    public static WriteResult FailureResult(string errorMessage, int? statusCode = null, long responseTimeMs = 0) =>
        new()
        {
            Success = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode,
            ResponseTimeMs = responseTimeMs
        };
}
