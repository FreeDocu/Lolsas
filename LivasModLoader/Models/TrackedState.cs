using System.Collections.ObjectModel;

namespace LivasModLoader.Models;

public class TrackedState
{
    public ObservableCollection<string> SyncedFiles { get; set; } = new();
    public DateTimeOffset? LastLaunchTimestamp { get; set; }
}
