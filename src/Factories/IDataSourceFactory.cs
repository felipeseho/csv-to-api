using n2n.Core;
using n2n.Models;

namespace n2n.Factories;

/// <summary>
///     Factory para criação de origens de dados
/// </summary>
public interface IDataSourceFactory
{
    /// <summary>
    ///     Cria uma origem de dados baseada na configuração
    /// </summary>
    Task<IDataSource> CreateAsync(DataSourceConfiguration configuration);

    /// <summary>
    ///     Verifica se a factory suporta o tipo especificado
    /// </summary>
    bool Supports(string sourceType);

    /// <summary>
    ///     Retorna os tipos suportados
    /// </summary>
    IEnumerable<string> GetSupportedTypes();
}
