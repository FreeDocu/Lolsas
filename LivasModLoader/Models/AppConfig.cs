using System.Collections.ObjectModel;

namespace LivasModLoader.Models;

public class AppConfig
{
    public string InstallationType { get; set; } = "NotSet";
    public string SteamAppId { get; set; } = string.Empty;
    public string SteamLaunchUri { get; set; } = string.Empty;
    public string EpicLaunchUri { get; set; } = string.Empty;
    public string GameExecutablePath { get; set; } = string.Empty;
    public string GameArguments { get; set; } = string.Empty;
    public string ModsLibraryPath { get; set; } = string.Empty;
    public string GameModsPath { get; set; } = string.Empty;
    public string ServerExecutablePath { get; set; } = string.Empty;
    public string AdditionalModActors { get; set; } = string.Empty;
    public string ServerBrowserBackendUrl { get; set; } = "https://servers.polehammer.net";
    public string AppUpdateEndpoint { get; set; } = string.Empty;
    public bool EnableAutomaticUpdates { get; set; }
    public bool EnableStarfield { get; set; } = true;
    public ObservableCollection<ServerPreset> ServerPresets { get; set; } = new();
    public ObservableCollection<LaunchProfile> LaunchProfiles { get; set; } = new();
}
