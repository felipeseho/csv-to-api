namespace n2n.Core;

/// <summary>
///     Interface para origens de dados
/// </summary>
public interface IDataSource : IDisposable
{
    /// <summary>
    ///     Nome do tipo de origem (CSV, Database, Queue, etc)
    /// </summary>
    string SourceType { get; }

    /// <summary>
    ///     Inicializa a origem de dados com configuração específica
    /// </summary>
    Task InitializeAsync(Dictionary<string, object> configuration);

    /// <summary>
    ///     Valida a configuração da origem
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync();

    /// <summary>
    ///     Retorna métricas da origem de dados
    /// </summary>
    SourceMetrics GetMetrics();

    /// <summary>
    ///     Lê dados da origem em formato de stream assíncrono
    /// </summary>
    IAsyncEnumerable<DataRecord> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Retorna estimativa do total de registros (se disponível)
    /// </summary>
    Task<long?> GetEstimatedCountAsync();
}
