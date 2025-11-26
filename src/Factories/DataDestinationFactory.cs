using n2n.Core;
using n2n.Models;
using n2n.Providers.Destinations;

namespace n2n.Factories;

/// <summary>
///     Factory para criação de destinos de dados
/// </summary>
public class DataDestinationFactory : IDataDestinationFactory
{
    private readonly Dictionary<string, Func<IDataDestination>> _destinationCreators =
        new(StringComparer.OrdinalIgnoreCase);

    public DataDestinationFactory()
    {
        // Registrar providers disponíveis
        RegisterProvider("API", () => new ApiDataDestination());
        // Futuramente: RegisterProvider("Database", () => new DatabaseDataDestination());
        // Futuramente: RegisterProvider("Queue", () => new QueueDataDestination());
        // Futuramente: RegisterProvider("File", () => new FileDataDestination());
    }

    public void RegisterProvider(string type, Func<IDataDestination> creator)
    {
        _destinationCreators[type] = creator;
    }

    public async Task<IDataDestination> CreateAsync(DataDestinationConfiguration configuration)
    {
        if (!_destinationCreators.TryGetValue(configuration.Type, out var creator))
        {
            throw new NotSupportedException(
                $"Tipo de destino '{configuration.Type}' não suportado. " +
                $"Tipos disponíveis: {string.Join(", ", GetSupportedTypes())}");
        }

        var destination = creator();
        await destination.InitializeAsync(configuration.Settings);

        var validation = await destination.ValidateConfigurationAsync();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Configuração inválida para destino '{configuration.Type}': " +
                string.Join(", ", validation.Errors));
        }

        return destination;
    }

    public bool Supports(string destinationType)
    {
        return _destinationCreators.ContainsKey(destinationType);
    }

    public IEnumerable<string> GetSupportedTypes()
    {
        return _destinationCreators.Keys;
    }
}
