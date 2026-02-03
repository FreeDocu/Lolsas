namespace LivasModLoader.Models;

public class ModMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Shared";
    public string Tags { get; set; } = string.Empty;
    public bool EnabledDefault { get; set; }
    public string UpdateSourceUrl { get; set; } = string.Empty;
    public DateTimeOffset? LastUpdated { get; set; }
    public string Hash { get; set; } = string.Empty;
}
