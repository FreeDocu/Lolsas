using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LivasModLoader.Models;
using LivasModLoader.Services;

namespace LivasModLoader.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly StateService _stateService;
    private readonly UpdateService _updateService;

    [ObservableProperty]
    private AppConfig config = new();

    public string[] InstallationTypes { get; } = { "Not set", "Steam", "EpicGamesStore" };

    public SettingsViewModel(ConfigService configService, StateService stateService, UpdateService updateService)
    {
        _configService = configService;
        _stateService = stateService;
        _updateService = updateService;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Config = await _configService.LoadAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await _configService.SaveAsync(Config);
    }

    [RelayCommand]
    private async Task CleanUpInstallationAsync()
    {
        await _stateService.CleanUpAsync();
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        await _updateService.CheckForAppUpdatesAsync();
    }
}
