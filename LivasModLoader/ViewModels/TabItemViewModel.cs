using CommunityToolkit.Mvvm.ComponentModel;

namespace LivasModLoader.ViewModels;

public partial class TabItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string header = string.Empty;

    [ObservableProperty]
    private object? content;
}
