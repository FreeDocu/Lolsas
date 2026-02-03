using System.Text.Json;
using LivasModLoader.Models;

namespace LivasModLoader.Services;

public class UpdateService
{
    private readonly ConfigService _configService;
    private readonly NotificationService _notificationService;

    public UpdateService(ConfigService configService, NotificationService notificationService)
    {
        _configService = configService;
        _notificationService = notificationService;
    }

    public async Task CheckForAppUpdatesAsync()
    {
        var config = await _configService.LoadAsync();
        if (string.IsNullOrWhiteSpace(config.AppUpdateEndpoint))
        {
            _notificationService.Push("Обновления", "Endpoint для обновлений не настроен.");
            return;
        }

        using var client = new HttpClient();
        var json = await client.GetStringAsync(config.AppUpdateEndpoint);
        using var document = JsonDocument.Parse(json);

        if (!document.RootElement.TryGetProperty("version", out var versionElement))
        {
            _notificationService.Push("Обновления", "Неверный формат манифеста.");
            return;
        }

        var latestVersion = versionElement.GetString() ?? "";
        var url = document.RootElement.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;

        _notificationService.Push("Обновления", $"Доступна версия: {latestVersion}. {url}");
    }
}
