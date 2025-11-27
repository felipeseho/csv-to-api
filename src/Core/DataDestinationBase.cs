using System.Diagnostics;

namespace n2n.Core;

/// <summary>
///     Classe base abstrata para destinos de dados
/// </summary>
public abstract class DataDestinationBase : IDataDestination
{
    protected Dictionary<string, object> Configuration { get; private set; } = new();
    protected readonly DestinationMetrics Metrics = new();
    protected readonly Stopwatch Timer = new();
    protected readonly List<long> ResponseTimes = new();
    private readonly object _responseTimesLock = new();
    private readonly object _metricsLock = new();

    public abstract string DestinationType { get; }

    public async Task InitializeAsync(Dictionary<string, object> configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        await OnInitializeAsync();
    }

    public async Task<ValidationResult> ValidateConfigurationAsync()
    {
        var result = new ValidationResult { IsValid = true };
        await OnValidateConfigurationAsync(result);
        return result;
    }

    public DestinationMetrics GetMetrics()
    {
        UpdateMetrics();
        return Metrics;
    }

    public abstract Task<WriteResult> WriteAsync(DataRecord record, CancellationToken cancellationToken = default);

    public virtual async Task<BatchWriteResult> WriteBatchAsync(IEnumerable<DataRecord> records,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchWriteResult();
        var batchTimer = Stopwatch.StartNew();

        foreach (var record in records)
        {
            var writeResult = await WriteAsync(record, cancellationToken);
            result.IndividualResults.Add(writeResult);
            result.TotalRecords++;

            if (writeResult.Success)
                result.SuccessCount++;
            else
                result.ErrorCount++;
        }

        batchTimer.Stop();
        result.TotalResponseTimeMs = batchTimer.ElapsedMilliseconds;

        return result;
    }

    protected abstract Task OnInitializeAsync();

    protected abstract Task OnValidateConfigurationAsync(ValidationResult result);

    protected T GetConfigValue<T>(string key, T defaultValue = default!)
    {
        if (Configuration.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                    return typedValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    protected void RecordResponse(long responseTimeMs, bool success)
    {
        lock (_responseTimesLock)
        {
            ResponseTimes.Add(responseTimeMs);
        }
        
        lock (_metricsLock)
        {
            Metrics.TotalRecordsWritten++;

            if (success)
                Metrics.SuccessCount++;
            else
                Metrics.ErrorCount++;
        }
    }

    protected void UpdateMetrics()
    {
        Metrics.ElapsedTime = Timer.Elapsed;

        if (Timer.Elapsed.TotalSeconds > 0)
        {
            Metrics.RecordsPerSecond = Metrics.TotalRecordsWritten / Timer.Elapsed.TotalSeconds;
        }

        lock (_responseTimesLock)
        {
            if (ResponseTimes.Count > 0)
            {
                Metrics.AverageResponseTimeMs = (long)ResponseTimes.Average();
                Metrics.MinResponseTimeMs = ResponseTimes.Min();
                Metrics.MaxResponseTimeMs = ResponseTimes.Max();
            }
        }
    }

    public virtual void Dispose()
    {
        Timer.Stop();
        GC.SuppressFinalize(this);
    }
}
