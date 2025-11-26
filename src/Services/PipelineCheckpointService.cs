using System.Text.Json;
using n2n.Models;

namespace n2n.Services;

/// <summary>
///     Serviço para gerenciamento de checkpoints de pipeline
/// </summary>
public class PipelineCheckpointService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    ///     Salva checkpoint de pipeline
    /// </summary>
    public async Task SaveCheckpointAsync(
        string checkpointDirectory,
        PipelineCheckpoint checkpoint)
    {
        await _semaphore.WaitAsync();
        try
        {
            Directory.CreateDirectory(checkpointDirectory);

            var fileName = $"checkpoint_{checkpoint.ExecutionId}.json";
            var filePath = Path.Combine(checkpointDirectory, fileName);

            checkpoint.UpdatedAt = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(checkpoint, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    ///     Carrega checkpoint existente
    /// </summary>
    public async Task<PipelineCheckpoint?> LoadCheckpointAsync(string checkpointDirectory, string executionId)
    {
        var fileName = $"checkpoint_{executionId}.json";
        var filePath = Path.Combine(checkpointDirectory, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<PipelineCheckpoint>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Lista todos os checkpoints disponíveis
    /// </summary>
    public List<PipelineCheckpoint> ListCheckpoints(string checkpointDirectory)
    {
        if (!Directory.Exists(checkpointDirectory))
        {
            return new List<PipelineCheckpoint>();
        }

        var checkpoints = new List<PipelineCheckpoint>();
        var files = Directory.GetFiles(checkpointDirectory, "checkpoint_*.json");

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var checkpoint = JsonSerializer.Deserialize<PipelineCheckpoint>(json);
                if (checkpoint != null)
                {
                    checkpoints.Add(checkpoint);
                }
            }
            catch
            {
                // Ignorar arquivos corrompidos
            }
        }

        return checkpoints.OrderByDescending(c => c.UpdatedAt).ToList();
    }

    /// <summary>
    ///     Remove checkpoint
    /// </summary>
    public void DeleteCheckpoint(string checkpointDirectory, string executionId)
    {
        var fileName = $"checkpoint_{executionId}.json";
        var filePath = Path.Combine(checkpointDirectory, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    ///     Gera um novo ID de execução
    /// </summary>
    public string GenerateExecutionId()
    {
        return Guid.NewGuid().ToString("N");
    }
}
