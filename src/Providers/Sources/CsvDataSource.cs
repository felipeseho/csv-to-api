using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;

namespace n2n.Providers.Sources;

/// <summary>
///     Origem de dados CSV
/// </summary>
public class CsvDataSource : Core.DataSourceBase
{
    private string _filePath = string.Empty;
    private string _delimiter = ",";
    private int _startLine = 1;
    private int? _maxLines;
    private int _batchSize = 100;
    private StreamReader? _reader;
    private CsvReader? _csvReader;
    private string[]? _headers;

    public override string SourceType => "CSV";

    public int BatchSize => _batchSize;

    protected override Task OnInitializeAsync()
    {
        _filePath = GetConfigValue<string>("FilePath", string.Empty);
        _delimiter = GetConfigValue<string>("Delimiter", ",");
        _startLine = GetConfigValue("StartLine", 1);
        _maxLines = GetConfigValue<int?>("MaxLines", null);
        _batchSize = GetConfigValue<int>("BatchSize", 100);

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Arquivo CSV não encontrado: {_filePath}");
        }

        Timer.Start();
        return Task.CompletedTask;
    }

    protected override Task OnValidateConfigurationAsync(Core.ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(_filePath))
        {
            result.AddError("FilePath é obrigatório");
        }
        else if (!File.Exists(_filePath))
        {
            result.AddError($"Arquivo não encontrado: {_filePath}");
        }

        if (string.IsNullOrWhiteSpace(_delimiter))
        {
            result.AddError("Delimiter é obrigatório");
        }

        if (_startLine < 1)
        {
            result.AddError("StartLine deve ser maior ou igual a 1");
        }

        if (_batchSize <= 0)
        {
            result.AddError("BatchSize deve ser maior que 0");
        }

        return Task.CompletedTask;
    }

    public override async Task<long?> GetEstimatedCountAsync()
    {
        try
        {
            // Se MaxLines está definido, usar como estimativa
            if (_maxLines.HasValue)
            {
                return _maxLines.Value;
            }

            using var reader = new StreamReader(_filePath);
            var count = 0L;
            while (await reader.ReadLineAsync() != null)
            {
                count++;
            }
            return count - 1; // Excluir cabeçalho
        }
        catch
        {
            return null;
        }
    }

    public override async IAsyncEnumerable<Core.DataRecord> ReadAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _reader = new StreamReader(_filePath);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = _delimiter,
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        _csvReader = new CsvReader(_reader, csvConfig);

        // Ler cabeçalho
        await _csvReader.ReadAsync();
        _csvReader.ReadHeader();
        _headers = _csvReader.HeaderRecord;

        if (_headers == null || _headers.Length == 0)
        {
            throw new InvalidOperationException("Arquivo CSV não contém cabeçalho");
        }

        var lineNumber = 1; // Linha 1 é o cabeçalho
        var processedCount = 0;

        // Pular linhas até a linha inicial
        while (lineNumber < _startLine && await _csvReader.ReadAsync())
        {
            lineNumber++;
        }

        // Ler dados
        while (await _csvReader.ReadAsync())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            // Verificar limite antes de processar
            if (_maxLines.HasValue && processedCount >= _maxLines.Value)
            {
                break;
            }

            lineNumber++;

            var data = new Dictionary<string, object>();
            foreach (var header in _headers)
            {
                data[header] = _csvReader.GetField(header) ?? string.Empty;
            }

            var record = new Core.DataRecord
            {
                RecordId = lineNumber.ToString(),
                Data = data,
                Metadata = new Dictionary<string, object>
                {
                    ["LineNumber"] = lineNumber,
                    ["SourceFile"] = _filePath
                }
            };

            Metrics.TotalRecordsRead++;
            UpdateMetrics();

            processedCount++;
            yield return record;
        }
    }

    public override void Dispose()
    {
        _csvReader?.Dispose();
        _reader?.Dispose();
        base.Dispose();
    }
}
