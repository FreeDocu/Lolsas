using System.Text.Json;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class StateService
{
    private readonly string _statePath;
    private readonly NotificationService _notificationService;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public StateService(NotificationService notificationService)
    {
        _notificationService = notificationService;
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LivasModLoader");
        Directory.CreateDirectory(root);
        _statePath = Path.Combine(root, "state.json");
    }

    public async Task<TrackedState> LoadAsync()
    {
        if (!File.Exists(_statePath))
        {
            var state = new TrackedState();
            await SaveAsync(state);
            return state;
        }

        await using var stream = File.OpenRead(_statePath);
        var stateFromFile = await JsonSerializer.DeserializeAsync<TrackedState>(stream, _options);
        return stateFromFile ?? new TrackedState();
    }

    public async Task SaveAsync(TrackedState state)
    {
        await using var stream = File.Create(_statePath);
        await JsonSerializer.SerializeAsync(stream, state, _options);
    }

    public async Task TrackFileAsync(string path)
    {
        var state = await LoadAsync();
        if (!state.SyncedFiles.Contains(path))
        {
            state.SyncedFiles.Add(path);
            await SaveAsync(state);
        }
    }

    public async Task CleanUpAsync()
    {
        var state = await LoadAsync();
        var removed = new List<string>();
        foreach (var file in state.SyncedFiles.ToList())
        {
            if (File.Exists(file))
            {
                File.Delete(file);
                removed.Add(file);
            }
        }
        state.SyncedFiles.Clear();
        await SaveAsync(state);

        var message = removed.Count == 0
            ? "Файлы для очистки не найдены."
            : $"Удалено файлов: {removed.Count}.";
        _notificationService.Push("Очистка установки", message);
    }
}
