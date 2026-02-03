using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LivasModLoader.Models;
using LivasModLoader.Services;

namespace LivasModLoader.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly NotificationService _notificationService;
    private readonly UpdateService _updateService;
    private readonly ConfigService _configService;

    public ObservableCollection<TabItemViewModel> Tabs { get; } = new();
    public ObservableCollection<NotificationMessage> Notifications => _notificationService.Notifications;
    public ObservableCollection<StarParticle> Stars { get; } = new();

    [ObservableProperty]
    private TabItemViewModel? selectedTab;

    [ObservableProperty]
    private bool isStarfieldEnabled;

    public Visibility StarfieldVisibility => IsStarfieldEnabled ? Visibility.Visible : Visibility.Collapsed;

    public MainViewModel(
        NotificationService notificationService,
        UpdateService updateService,
        ConfigService configService,
        LauncherViewModel launcherViewModel,
        ModManagerViewModel modManagerViewModel,
        ServerViewModel serverViewModel,
        SettingsViewModel settingsViewModel)
    {
        _notificationService = notificationService;
        _updateService = updateService;
        _configService = configService;

        Tabs.Add(new TabItemViewModel { Header = "Лаунчер", Content = launcherViewModel });
        Tabs.Add(new TabItemViewModel { Header = "Менеджер модов", Content = modManagerViewModel });
        Tabs.Add(new TabItemViewModel { Header = "Сервер", Content = serverViewModel });
        Tabs.Add(new TabItemViewModel { Header = "Настройки", Content = settingsViewModel });
        SelectedTab = Tabs[0];

        LoadStateAsync();
        BuildStarfield();
    }

    private async void LoadStateAsync()
    {
        var config = await _configService.LoadAsync();
        IsStarfieldEnabled = config.EnableStarfield;
    }

    partial void OnIsStarfieldEnabledChanged(bool value)
    {
        _ = SaveStarfieldPreferenceAsync(value);
        OnPropertyChanged(nameof(StarfieldVisibility));
    }

    private async Task SaveStarfieldPreferenceAsync(bool value)
    {
        var config = await _configService.LoadAsync();
        config.EnableStarfield = value;
        await _configService.SaveAsync(config);
    }

    private void BuildStarfield()
    {
        var random = new Random();
        for (var i = 0; i < 140; i++)
        {
            Stars.Add(new StarParticle
            {
                X = random.NextDouble() * 900 + 50,
                Y = random.NextDouble() * 700 + 50,
                Size = random.NextDouble() * 2.4 + 1,
                Opacity = random.NextDouble() * 0.6 + 0.3
            });
        }
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        await _updateService.CheckForAppUpdatesAsync();
    }
}
