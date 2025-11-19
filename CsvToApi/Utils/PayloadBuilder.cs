using CsvToApi.Models;

namespace CsvToApi.Utils;

/// <summary>
/// Utilitário para construção de payloads da API
/// </summary>
public static class PayloadBuilder
{
    /// <summary>
    /// Constrói o payload JSON a partir de um registro CSV
    /// </summary>
    public static Dictionary<string, object> BuildApiPayload(CsvRecord record, List<ApiMapping> mappings)
    {
        var payload = new Dictionary<string, object>();

        foreach (var mapping in mappings)
        {
            if (!record.Data.TryGetValue(mapping.CsvColumn, out var value))
            {
                continue;
            }

            // Suportar atributos aninhados (ex: "address.street")
            var parts = mapping.Attribute.Split('.');
            if (parts.Length == 1)
            {
                payload[mapping.Attribute] = value;
            }
            else
            {
                // Criar estrutura aninhada
                var current = payload;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current.ContainsKey(parts[i]))
                    {
                        current[parts[i]] = new Dictionary<string, object>();
                    }
                    current = (Dictionary<string, object>)current[parts[i]];
                }
                current[parts[^1]] = value;
            }
        }

        return payload;
    }
}

