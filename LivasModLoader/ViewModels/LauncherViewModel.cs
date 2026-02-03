using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LivasModLoader.Models;
using LivasModLoader.Services;

namespace LivasModLoader.ViewModels;

public partial class LauncherViewModel : ObservableObject
{
    private readonly LauncherService _launcherService;
    private readonly ModService _modService;
    private readonly ConfigService _configService;

    public ObservableCollection<LaunchProfile> Profiles { get; } = new();

    [ObservableProperty]
    private string selectedPlatform = "Steam";

    public LauncherViewModel(LauncherService launcherService, ModService modService, ConfigService configService)
    {
        _launcherService = launcherService;
        _modService = modService;
        _configService = configService;

        Profiles.Add(new LaunchProfile { Name = "Chivalry 2", SyncModsBeforeLaunch = false });
        Profiles.Add(new LaunchProfile { Name = "Chivalry 2 с модами", SyncModsBeforeLaunch = true });
        Profiles.Add(new LaunchProfile { Name = "Chivalry 2 Unchained", SyncModsBeforeLaunch = true, AdditionalArguments = "-unchained" });
    }

    [RelayCommand]
    private async Task LaunchAsync(LaunchProfile profile)
    {
        if (profile.SyncModsBeforeLaunch)
        {
            await _modService.SyncEnabledModsAsync();
        }

        var config = await _configService.LoadAsync();
        await _launcherService.LaunchAsync(profile, config, SelectedPlatform);
    }
}
