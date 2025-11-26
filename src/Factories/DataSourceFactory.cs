using n2n.Core;
using n2n.Models;
using n2n.Providers.Sources;

namespace n2n.Factories;

/// <summary>
///     Factory para criação de origens de dados
/// </summary>
public class DataSourceFactory : IDataSourceFactory
{
    private readonly Dictionary<string, Func<IDataSource>> _sourceCreators = new(StringComparer.OrdinalIgnoreCase);

    public DataSourceFactory()
    {
        // Registrar providers disponíveis
        RegisterProvider("CSV", () => new CsvDataSource());
        // Futuramente: RegisterProvider("Database", () => new DatabaseDataSource());
        // Futuramente: RegisterProvider("Queue", () => new QueueDataSource());
        // Futuramente: RegisterProvider("API", () => new ApiDataSource());
    }

    public void RegisterProvider(string type, Func<IDataSource> creator)
    {
        _sourceCreators[type] = creator;
    }

    public async Task<IDataSource> CreateAsync(DataSourceConfiguration configuration)
    {
        if (!_sourceCreators.TryGetValue(configuration.Type, out var creator))
        {
            throw new NotSupportedException(
                $"Tipo de origem '{configuration.Type}' não suportado. " +
                $"Tipos disponíveis: {string.Join(", ", GetSupportedTypes())}");
        }

        var source = creator();
        await source.InitializeAsync(configuration.Settings);

        var validation = await source.ValidateConfigurationAsync();
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Configuração inválida para origem '{configuration.Type}': " +
                string.Join(", ", validation.Errors));
        }

        return source;
    }

    public bool Supports(string sourceType)
    {
        return _sourceCreators.ContainsKey(sourceType);
    }

    public IEnumerable<string> GetSupportedTypes()
    {
        return _sourceCreators.Keys;
    }
}
