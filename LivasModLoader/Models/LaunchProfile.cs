namespace LivasModLoader.Models;

public class LaunchProfile
{
    public string Name { get; set; } = string.Empty;
    public bool SyncModsBeforeLaunch { get; set; }
    public string AdditionalArguments { get; set; } = string.Empty;
}
