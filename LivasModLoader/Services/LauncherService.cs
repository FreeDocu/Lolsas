using System.Diagnostics;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class LauncherService
{
    private readonly NotificationService _notificationService;

    public LauncherService(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task LaunchAsync(LaunchProfile profile, AppConfig config, string platform)
    {
        await Task.Run(() =>
        {
            if (!ValidateConfig(config, platform, out var launchTarget))
            {
                return;
            }

            var args = string.Join(" ", new[]
            {
                config.GameArguments,
                config.AdditionalModActors,
                profile.AdditionalArguments
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

            _notificationService.Push("Запуск игры", $"Профиль: {profile.Name}. Платформа: {platform}.");

            var startInfo = new ProcessStartInfo
            {
                FileName = launchTarget,
                Arguments = args,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        });
    }

    private bool ValidateConfig(AppConfig config, string platform, out string launchTarget)
    {
        launchTarget = string.Empty;
        if (platform.Equals("Steam", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(config.SteamAppId))
            {
                launchTarget = $"steam://run/{config.SteamAppId}";
                return true;
            }

            if (!string.IsNullOrWhiteSpace(config.SteamLaunchUri))
            {
                launchTarget = config.SteamLaunchUri;
                return true;
            }
        }

        if (platform.Equals("Epic", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(config.EpicLaunchUri))
            {
                launchTarget = config.EpicLaunchUri;
                return true;
            }
        }

        if (!string.IsNullOrWhiteSpace(config.GameExecutablePath) && File.Exists(config.GameExecutablePath))
        {
            launchTarget = config.GameExecutablePath;
            return true;
        }

        _notificationService.Push("Ошибка запуска", "Не найден путь запуска. Проверьте настройки платформы.");
        return false;
    }
}
