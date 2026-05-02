using System.Threading;
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
    private static Mutex? _singleInstanceMutex;
    private ServiceProvider? _serviceProvider;
    private TrayIconManager? _trayManager;
    private SettingsWindow? _settingsWindow;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single instance check
        const string mutexName = "Global\\TtsCommunicationTool_SingleInstance";
        _singleInstanceMutex = new Mutex(true, mutexName, out var createdNew);
        if (!createdNew)
        {
            System.Windows.MessageBox.Show(
                "TTS Swirlotl is already running.",
                "Already Running",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

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

        // Show splash screen if enabled
        SplashWindow? splash = null;
        if (config.CurrentConfig.GeneralSettings.ShowSplashScreen)
        {
            splash = new SplashWindow();
            splash.ShowSplash();
        }

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

        // Wire up phrase cache service (late-bind to avoid circular DI)
        var phraseService = _serviceProvider.GetRequiredService<IPhraseService>() as Infrastructure.Phrases.PhraseService;
        var phraseCache = _serviceProvider.GetRequiredService<IPhraseCacheService>();
        phraseService?.SetCacheService(phraseCache);

        // Pre-generate cache for any phrases that don't have cached audio yet
        if (tts.IsInitialized)
        {
            _ = Task.Run(async () =>
            {
                foreach (var phrase in _serviceProvider.GetRequiredService<IPhraseService>().GetAll())
                {
                    if (!phraseCache.HasCache(phrase.Id))
                        await phraseCache.GenerateCacheAsync(phrase);
                }
            });
        }

        // Set up tray icon
        _trayManager = _serviceProvider.GetRequiredService<TrayIconManager>();
        _trayManager.Initialize();
        _trayManager.SettingsRequested += (_, _) => OpenSettings();
        log.Info("Tray icon initialized.");

        // Create invisible host window for hotkey messages
        var hotkeyHost = _serviceProvider.GetRequiredService<HotkeyHostWindow>();
        hotkeyHost.SettingsRequested += (_, _) => OpenSettings();
        hotkeyHost.Show();
        hotkeyHost.Hide();

        // Wire overlay gear-button → open settings
        var overlayCoordinator = _serviceProvider.GetRequiredService<IOverlayCoordinator>();
        overlayCoordinator.SettingsRequested += (_, _) => OpenSettings();

        log.Info("Application started successfully.");

        // Close splash now that everything is loaded
        splash?.CloseNow();

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

        // Singleton: if settings already open, just activate
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        var vm = _serviceProvider.GetRequiredService<SettingsViewModel>();
        vm.IsFirstRun = isFirstRun;

        // Refresh all hotkeys after settings are saved
        vm.Saved += (_, _) =>
        {
            var host = _serviceProvider.GetRequiredService<IHotkeyHost>();
            host.RefreshAllHotkeys();
        };

        _settingsWindow = new SettingsWindow(vm);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
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

        if (_singleInstanceMutex is not null)
        {
            _singleInstanceMutex.ReleaseMutex();
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }

        base.OnExit(e);
    }
}
