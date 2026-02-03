using System.IO;
using System.Windows;
using LivasModLoader.Services;
using LivasModLoader.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace LivasModLoader;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LivasModLoader");
        Directory.CreateDirectory(appData);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(appData, "livasmodloader.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(new SerilogLoggerFactory(Log.Logger));
        services.AddSingleton<ConfigService>();
        services.AddSingleton<StateService>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<LauncherService>();
        services.AddSingleton<ModService>();
        services.AddSingleton<ServerService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LauncherViewModel>();
        services.AddSingleton<ModManagerViewModel>();
        services.AddSingleton<ServerViewModel>();
        services.AddSingleton<SettingsViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = new MainWindow
        {
            DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
