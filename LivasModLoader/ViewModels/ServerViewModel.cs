using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LivasModLoader.Models;
using LivasModLoader.Services;

namespace LivasModLoader.ViewModels;

public partial class ServerViewModel : ObservableObject
{
    private readonly ServerService _serverService;

    public ObservableCollection<string> MapOptions { get; } = new()
    {
        "FFA_FightingPit",
        "TDM_TournamentGround",
        "TO_Falmire",
        "TO_Coxwell",
        "TO_Galencourt",
        "LTS_Rudhelm",
        "TO_Rudhelm",
        "TO_Baudwyn",
        "TO_Askandir"
    };

    [ObservableProperty]
    private ServerPreset currentPreset = new();

    [ObservableProperty]
    private string processStatus = "Остановлен";

    [ObservableProperty]
    private int? processId;

    [ObservableProperty]
    private string previewArguments = string.Empty;

    public ServerViewModel(ServerService serverService)
    {
        _serverService = serverService;
        CurrentPreset.StartingMap = MapOptions[0];
        UpdatePreview();
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        PreviewArguments = _serverService.BuildArguments(CurrentPreset);
    }

    [RelayCommand]
    private async Task LaunchServerAsync()
    {
        var result = await _serverService.LaunchServerAsync(CurrentPreset, false);
        ProcessStatus = result.Status;
        ProcessId = result.ProcessId;
    }

    [RelayCommand]
    private async Task LaunchHeadlessAsync()
    {
        var result = await _serverService.LaunchServerAsync(CurrentPreset, true);
        ProcessStatus = result.Status;
        ProcessId = result.ProcessId;
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await _serverService.StopServerAsync();
        ProcessStatus = "Остановлен";
        ProcessId = null;
    }
}
