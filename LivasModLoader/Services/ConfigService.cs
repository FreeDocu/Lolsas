using System.Text.Json;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class ConfigService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public ConfigService()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LivasModLoader");
        Directory.CreateDirectory(root);
        _configPath = Path.Combine(root, "config.json");
    }

    public async Task<AppConfig> LoadAsync()
    {
        if (!File.Exists(_configPath))
        {
            var config = new AppConfig
            {
                ModsLibraryPath = Path.Combine(Path.GetDirectoryName(_configPath)!, "ModsLibrary")
            };
            await SaveAsync(config);
            return config;
        }

        await using var stream = File.OpenRead(_configPath);
        var configFromFile = await JsonSerializer.DeserializeAsync<AppConfig>(stream, _options);
        return configFromFile ?? new AppConfig();
    }

    public async Task SaveAsync(AppConfig config)
    {
        var folder = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        await using var stream = File.Create(_configPath);
        await JsonSerializer.SerializeAsync(stream, config, _options);
    }
}
