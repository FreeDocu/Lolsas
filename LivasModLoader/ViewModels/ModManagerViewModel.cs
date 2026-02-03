using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LivasModLoader.Models;
using LivasModLoader.Services;

namespace LivasModLoader.ViewModels;

public partial class ModManagerViewModel : ObservableObject
{
    private readonly ModService _modService;

    public ObservableCollection<ModEntry> Mods => _modService.Mods;

    [ObservableProperty]
    private ModEntry? selectedMod;

    public ModManagerViewModel(ModService modService)
    {
        _modService = modService;
        _ = _modService.RefreshAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await _modService.RefreshAsync();
    }

    [RelayCommand]
    private async Task UpdateModsAsync()
    {
        await _modService.UpdateModsAsync();
    }

    [RelayCommand]
    private async Task ToggleEnabledAsync(ModEntry mod)
    {
        mod.IsEnabled = !mod.IsEnabled;
        if (mod.IsEnabled)
        {
            await _modService.EnableModAsync(mod);
        }
        else
        {
            await _modService.DisableModAsync(mod);
        }
    }

    [RelayCommand]
    private async Task DeleteFromLibraryAsync(ModEntry mod)
    {
        await _modService.DeleteFromLibraryAsync(mod);
    }

    [RelayCommand]
    private async Task ImportModAsync()
    {
        await _modService.ImportModAsync();
    }
}
