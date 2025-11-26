using n2n.Core;
using n2n.Models;

namespace n2n.Factories;

/// <summary>
///     Factory para criação de destinos de dados
/// </summary>
public interface IDataDestinationFactory
{
    /// <summary>
    ///     Cria um destino de dados baseado na configuração
    /// </summary>
    Task<IDataDestination> CreateAsync(DataDestinationConfiguration configuration);

    /// <summary>
    ///     Verifica se a factory suporta o tipo especificado
    /// </summary>
    bool Supports(string destinationType);

    /// <summary>
    ///     Retorna os tipos suportados
    /// </summary>
    IEnumerable<string> GetSupportedTypes();
}
