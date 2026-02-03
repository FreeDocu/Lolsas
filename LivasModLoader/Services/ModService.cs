using System.Collections.ObjectModel;
using System.Text.Json;
using LivasModLoader.Models;
using Microsoft.Win32;

namespace LivasModLoader.Services;

public class ModService
{
    private readonly ConfigService _configService;
    private readonly NotificationService _notificationService;
    private readonly StateService _stateService;
    private FileSystemWatcher? _libraryWatcher;
    private FileSystemWatcher? _gameWatcher;

    public ObservableCollection<ModEntry> Mods { get; } = new();

    public ModService(ConfigService configService, NotificationService notificationService, StateService stateService)
    {
        _configService = configService;
        _notificationService = notificationService;
        _stateService = stateService;
    }

    public async Task RefreshAsync()
    {
        var config = await _configService.LoadAsync();
        if (string.IsNullOrWhiteSpace(config.ModsLibraryPath))
        {
            return;
        }

        Directory.CreateDirectory(config.ModsLibraryPath);
        Mods.Clear();
        var libraryMods = Directory.GetFiles(config.ModsLibraryPath, "*.pak");

        foreach (var modPath in libraryMods)
        {
            var metadata = await LoadMetadataAsync(modPath);
            var isEnabled = false;
            if (!string.IsNullOrWhiteSpace(config.GameModsPath) && Directory.Exists(config.GameModsPath))
            {
                var target = Path.Combine(config.GameModsPath, Path.GetFileName(modPath));
                isEnabled = File.Exists(target);
            }

            Mods.Add(new ModEntry
            {
                FileName = Path.GetFileName(modPath),
                FullPath = modPath,
                Metadata = metadata,
                IsEnabled = isEnabled
            });
        }

        SetupWatchers(config);
        _notificationService.Push("Моды", "Список модов обновлён.");
    }

    public async Task ImportModAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PAK files (*.pak)|*.pak",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var config = await _configService.LoadAsync();
        Directory.CreateDirectory(config.ModsLibraryPath);
        var destination = Path.Combine(config.ModsLibraryPath, Path.GetFileName(dialog.FileName));

        await Task.Run(() => File.Copy(dialog.FileName, destination, true));
        await SaveMetadataAsync(destination, new ModMetadata { Name = Path.GetFileNameWithoutExtension(destination) });
        await RefreshAsync();
        _notificationService.Push("Импорт", "Мод добавлен в библиотеку.");
    }

    public async Task EnableModAsync(ModEntry mod)
    {
        var config = await _configService.LoadAsync();
        if (!ValidatePaths(config))
        {
            return;
        }

        var target = Path.Combine(config.GameModsPath, mod.FileName);
        await CopyAtomicAsync(mod.FullPath, target);
        await _stateService.TrackFileAsync(target);
        _notificationService.Push("Моды", $"Включён мод: {mod.Metadata.Name}");
    }

    public async Task DisableModAsync(ModEntry mod)
    {
        var config = await _configService.LoadAsync();
        if (!ValidatePaths(config))
        {
            return;
        }

        var target = Path.Combine(config.GameModsPath, mod.FileName);
        await Task.Run(() =>
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }
        });
        _notificationService.Push("Моды", $"Отключён мод: {mod.Metadata.Name}");
    }

    public async Task DeleteFromLibraryAsync(ModEntry mod)
    {
        await Task.Run(() =>
        {
            if (File.Exists(mod.FullPath))
            {
                File.Delete(mod.FullPath);
            }
            var metadataPath = mod.FullPath + ".json";
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
        });
        Mods.Remove(mod);
        _notificationService.Push("Моды", $"Мод удалён из библиотеки: {mod.Metadata.Name}");
    }

    public async Task SyncEnabledModsAsync()
    {
        var config = await _configService.LoadAsync();
        if (!ValidatePaths(config))
        {
            return;
        }

        foreach (var mod in Mods.Where(m => m.IsEnabled))
        {
            var target = Path.Combine(config.GameModsPath, mod.FileName);
            await CopyAtomicAsync(mod.FullPath, target);
            await _stateService.TrackFileAsync(target);
        }

        _notificationService.Push("Моды", "Синхронизация модов завершена.");
    }

    public async Task UpdateModsAsync()
    {
        using var client = new HttpClient();
        foreach (var mod in Mods)
        {
            if (string.IsNullOrWhiteSpace(mod.Metadata.UpdateSourceUrl))
            {
                continue;
            }

            if (mod.Metadata.UpdateSourceUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateFromManifestAsync(client, mod);
            }
            else
            {
                await DownloadUpdateAsync(client, mod.Metadata.UpdateSourceUrl, mod.FullPath, mod);
            }
        }

        _notificationService.Push("Обновления", "Проверка обновлений модов завершена.");
    }

    private async Task UpdateFromManifestAsync(HttpClient client, ModEntry mod)
    {
        var json = await client.GetStringAsync(mod.Metadata.UpdateSourceUrl);
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("files", out var filesElement) || filesElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var file in filesElement.EnumerateArray())
        {
            var url = file.GetProperty("url").GetString();
            var hash = file.TryGetProperty("hash", out var hashElement) ? hashElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            await DownloadUpdateAsync(client, url, mod.FullPath, mod, hash);
        }
    }

    private async Task DownloadUpdateAsync(HttpClient client, string url, string targetPath, ModEntry mod, string? hash = null)
    {
        var tempFile = Path.GetTempFileName();
        var bytes = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(tempFile, bytes);

        if (!string.IsNullOrWhiteSpace(hash))
        {
            var calculated = await HashFileAsync(tempFile);
            if (!string.Equals(calculated, hash, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tempFile);
                _notificationService.Push("Обновления", $"Хэш не совпал для {mod.Metadata.Name}.");
                return;
            }
        }

        await CopyAtomicAsync(tempFile, targetPath);
        File.Delete(tempFile);
        mod.Metadata.LastUpdated = DateTimeOffset.Now;
        await SaveMetadataAsync(targetPath, mod.Metadata);
    }

    private async Task<string> HashFileAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash);
    }

    private async Task<ModMetadata> LoadMetadataAsync(string modPath)
    {
        var metadataPath = modPath + ".json";
        if (!File.Exists(metadataPath))
        {
            return new ModMetadata { Name = Path.GetFileNameWithoutExtension(modPath) };
        }

        await using var stream = File.OpenRead(metadataPath);
        var metadata = await JsonSerializer.DeserializeAsync<ModMetadata>(stream);
        return metadata ?? new ModMetadata();
    }

    private async Task SaveMetadataAsync(string modPath, ModMetadata metadata)
    {
        var metadataPath = modPath + ".json";
        await using var stream = File.Create(metadataPath);
        await JsonSerializer.SerializeAsync(stream, metadata, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task CopyAtomicAsync(string source, string destination)
    {
        var tempFile = destination + ".tmp";
        await Task.Run(() => File.Copy(source, tempFile, true));
        File.Move(tempFile, destination, true);
    }

    private bool ValidatePaths(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.GameModsPath) || !Directory.Exists(config.GameModsPath))
        {
            _notificationService.Push("Ошибка", "Папка модов игры не настроена.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(config.ModsLibraryPath) || !Directory.Exists(config.ModsLibraryPath))
        {
            _notificationService.Push("Ошибка", "Библиотека модов не найдена.");
            return false;
        }

        return true;
    }

    private void SetupWatchers(AppConfig config)
    {
        _libraryWatcher?.Dispose();
        _gameWatcher?.Dispose();

        _libraryWatcher = new FileSystemWatcher(config.ModsLibraryPath, "*.pak")
        {
            EnableRaisingEvents = true
        };
        _libraryWatcher.Changed += async (_, _) => await RefreshAsync();
        _libraryWatcher.Created += async (_, _) => await RefreshAsync();
        _libraryWatcher.Deleted += async (_, _) => await RefreshAsync();

        if (Directory.Exists(config.GameModsPath))
        {
            _gameWatcher = new FileSystemWatcher(config.GameModsPath, "*.pak")
            {
                EnableRaisingEvents = true
            };
            _gameWatcher.Changed += async (_, _) => await RefreshAsync();
            _gameWatcher.Created += async (_, _) => await RefreshAsync();
            _gameWatcher.Deleted += async (_, _) => await RefreshAsync();
        }
    }
}
