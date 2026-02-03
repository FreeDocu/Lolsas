namespace LivasModLoader.Models;

public class ModEntry
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public ModMetadata Metadata { get; set; } = new();
    public bool IsEnabled { get; set; }
}
