using System.Diagnostics;

namespace n2n.Core;

/// <summary>
///     Classe base abstrata para origens de dados
/// </summary>
public abstract class DataSourceBase : IDataSource
{
    protected Dictionary<string, object> Configuration { get; private set; } = new();
    protected readonly SourceMetrics Metrics = new();
    protected readonly Stopwatch Timer = new();

    public abstract string SourceType { get; }

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

    public SourceMetrics GetMetrics() => Metrics;

    public abstract IAsyncEnumerable<DataRecord> ReadAsync(CancellationToken cancellationToken = default);

    public abstract Task<long?> GetEstimatedCountAsync();

    protected abstract Task OnInitializeAsync();

    protected abstract Task OnValidateConfigurationAsync(ValidationResult result);

    protected T GetConfigValue<T>(string key, T defaultValue = default!)
    {
        if (Configuration.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    protected void UpdateMetrics()
    {
        Metrics.ElapsedTime = Timer.Elapsed;
        if (Timer.Elapsed.TotalSeconds > 0)
        {
            Metrics.RecordsPerSecond = Metrics.TotalRecordsRead / Timer.Elapsed.TotalSeconds;
        }
    }

    public virtual void Dispose()
    {
        Timer.Stop();
        GC.SuppressFinalize(this);
    }
}
