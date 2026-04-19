using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Infrastructure.Config;
using TtsCommunicationTool.UI.ViewModels;
using TtsCommunicationTool.UI.Views;

namespace TtsCommunicationTool.App;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private TrayIconManager? _trayManager;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var services = new ServiceCollection();
        ServiceRegistration.Configure(services);
        _serviceProvider = services.BuildServiceProvider();

        var log = _serviceProvider.GetRequiredService<ILoggingService>();
        log.Info("Application starting...");

        // Load config (with recovery support)
        var config = _serviceProvider.GetRequiredService<IConfigService>();
        await config.LoadAsync();
        log.Info("Configuration loaded.");

        // Check for config recovery notification
        var jsonConfig = config as JsonConfigService;
        var isFirstRun = jsonConfig?.IsFirstRun ?? false;
        var wasRecovered = jsonConfig?.WasRecovered ?? false;

        if (wasRecovered)
        {
            var notify = _serviceProvider.GetRequiredService<INotificationService>();
            notify.ShowWarning("Configuration file was corrupt and has been reset to defaults. A backup was saved.");
            log.Warn("Config was recovered from corrupt state.");
        }

        // Initialize TTS
        var tts = _serviceProvider.GetRequiredService<ITtsService>();
        try
        {
            await tts.InitializeAsync();
            log.Info("TTS engine initialized.");
        }
        catch (Exception ex)
        {
            log.Error("TTS initialization failed.", ex);
            var notify = _serviceProvider.GetRequiredService<INotificationService>();
            notify.ShowError($"TTS engine failed to initialize: {ex.Message}");
        }

        // Set up tray icon
        _trayManager = _serviceProvider.GetRequiredService<TrayIconManager>();
        _trayManager.Initialize();
        _trayManager.SettingsRequested += (_, _) => OpenSettings();
        log.Info("Tray icon initialized.");

        // Create invisible host window for hotkey messages
        var hotkeyHost = _serviceProvider.GetRequiredService<HotkeyHostWindow>();
        hotkeyHost.Show();
        hotkeyHost.Hide();

        log.Info("Application started successfully.");

        // First-run: auto-open settings
        if (isFirstRun)
        {
            log.Info("First run detected — opening settings.");
            OpenSettings(isFirstRun: true);
        }
    }

    private void OpenSettings(bool isFirstRun = false)
    {
        if (_serviceProvider is null) return;

        var vm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        vm.IsFirstRun = isFirstRun;

        var window = new SettingsWindow(vm);
        window.Show();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        LogFatalError("AppDomain.UnhandledException", ex);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogFatalError("DispatcherUnhandledException", e.Exception);
        e.Handled = true; // Prevent crash — log and notify

        var notify = _serviceProvider?.GetService<INotificationService>();
        notify?.ShowError($"An unexpected error occurred: {e.Exception.Message}");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogFatalError("UnobservedTaskException", e.Exception);
        e.SetObserved();
    }

    private void LogFatalError(string source, Exception? ex)
    {
        var log = _serviceProvider?.GetService<ILoggingService>();
        log?.Error($"[{source}] {ex?.Message}", ex);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var log = _serviceProvider?.GetService<ILoggingService>();
        log?.Info("Application shutting down...");

        _trayManager?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
