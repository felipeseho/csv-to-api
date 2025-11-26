namespace n2n.Core;

/// <summary>
///     Interface para destinos de dados
/// </summary>
public interface IDataDestination : IDisposable
{
    /// <summary>
    ///     Nome do tipo de destino (API, Database, Queue, etc)
    /// </summary>
    string DestinationType { get; }

    /// <summary>
    ///     Inicializa o destino de dados com configuração específica
    /// </summary>
    Task InitializeAsync(Dictionary<string, object> configuration);

    /// <summary>
    ///     Valida a configuração do destino
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync();

    /// <summary>
    ///     Retorna métricas do destino de dados
    /// </summary>
    DestinationMetrics GetMetrics();

    /// <summary>
    ///     Escreve um único registro
    /// </summary>
    Task<WriteResult> WriteAsync(DataRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Escreve um lote de registros (implementação opcional, pode usar WriteAsync por padrão)
    /// </summary>
    Task<BatchWriteResult> WriteBatchAsync(IEnumerable<DataRecord> records, CancellationToken cancellationToken = default);
}
