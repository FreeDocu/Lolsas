using System.Diagnostics;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class ServerService
{
    private readonly ConfigService _configService;
    private readonly NotificationService _notificationService;
    private Process? _process;

    public ServerService(ConfigService configService, NotificationService notificationService)
    {
        _configService = configService;
        _notificationService = notificationService;
    }

    public string BuildArguments(ServerPreset preset)
    {
        return string.Join(" ", new[]
        {
            $"-ServerName=\"{preset.Name}\"",
            $"-ServerDescription=\"{preset.Description}\"",
            string.IsNullOrWhiteSpace(preset.Password) ? string.Empty : $"-ServerPassword=\"{preset.Password}\"",
            preset.UseBackendBanList ? "-UseBackendBanList" : string.Empty,
            $"-Map={preset.StartingMap}",
            $"-Port={preset.GamePort}",
            $"-RconPort={preset.RconPort}",
            $"-A2SPort={preset.A2SPort}",
            $"-PingPort={preset.PingPort}"
        }.Where(arg => !string.IsNullOrWhiteSpace(arg)));
    }

    public async Task<(string Status, int? ProcessId)> LaunchServerAsync(ServerPreset preset, bool headless)
    {
        var config = await _configService.LoadAsync();
        if (string.IsNullOrWhiteSpace(config.ServerExecutablePath) || !File.Exists(config.ServerExecutablePath))
        {
            _notificationService.Push("Сервер", "Путь к серверному исполняемому файлу не указан.");
            return ("Остановлен", null);
        }

        var args = BuildArguments(preset);
        if (headless)
        {
            args += " -Headless";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = config.ServerExecutablePath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _process = Process.Start(startInfo);
        _notificationService.Push("Сервер", "Сервер запущен.");
        return ("Запущен", _process?.Id);
    }

    public async Task StopServerAsync()
    {
        await Task.Run(() =>
        {
            if (_process is { HasExited: false })
            {
                _process.Kill();
                _process.Dispose();
                _process = null;
            }
        });
        _notificationService.Push("Сервер", "Сервер остановлен.");
    }
}
