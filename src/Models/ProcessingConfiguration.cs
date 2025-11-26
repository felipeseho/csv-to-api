namespace n2n.Models;

public class ProcessingConfiguration
{
    public string LogDirectory { get; set; } = "logs";
    public string CheckpointDirectory { get; set; } = "checkpoints";
    public int CheckpointIntervalSeconds { get; set; } = 30;
}
