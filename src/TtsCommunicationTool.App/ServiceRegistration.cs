using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.Infrastructure.Audio;
using TtsCommunicationTool.Infrastructure.Config;
using TtsCommunicationTool.Infrastructure.Logging;
using TtsCommunicationTool.Infrastructure.Phrases;
using TtsCommunicationTool.Infrastructure.TextReplacement;
using TtsCommunicationTool.Infrastructure.Transcript;
using TtsCommunicationTool.Infrastructure.Themes;
using TtsCommunicationTool.Infrastructure.Tts;
using TtsCommunicationTool.UI.Services;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.App;

public static class ServiceRegistration
{
    public static void Configure(IServiceCollection services)
    {
        // State (singletons)
        services.AddSingleton<AppRuntimeState>();
        services.AddSingleton<OverlayState>();
        services.AddSingleton<PlaybackState>();
        services.AddSingleton<RecentMessagesState>();

        // Infrastructure services
        services.AddSingleton<ILoggingService, FileLoggingService>();
        services.AddSingleton<IConfigService, JsonConfigService>();
        services.AddSingleton<IAudioDeviceService, WasapiAudioDeviceService>();
        services.AddSingleton<IAudioRouterService, DualOutputAudioRouter>();
        services.AddSingleton<HttpClient>();
        services.AddSingleton<KokoroTtsService>();
        services.AddSingleton<ElevenLabsTtsService>();
        services.AddSingleton<TtsRouter>();
        services.AddSingleton<ITtsService>(sp => sp.GetRequiredService<TtsRouter>());
        services.AddSingleton<IPhraseService, PhraseService>();
        services.AddSingleton<IPhraseCacheService, PhraseCacheService>();
        services.AddSingleton<INotificationService, WpfNotificationService>();
        services.AddSingleton<ITextReplacementService, TextReplacementService>();
        services.AddSingleton<ITranscriptService, TranscriptService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IColorPickerService, WinFormsColorPickerService>();

        // App-level services
        services.AddSingleton<TrayIconManager>();
        services.AddSingleton<HotkeyHostWindow>();
        services.AddSingleton<IHotkeyHost>(sp => sp.GetRequiredService<HotkeyHostWindow>());
        services.AddSingleton<OverlayCoordinator>();
        services.AddSingleton<IOverlayCoordinator>(sp => sp.GetRequiredService<OverlayCoordinator>());

        // ViewModels
        services.AddTransient<OverlayViewModel>();
        services.AddTransient<GeneralSettingsViewModel>();
        services.AddTransient<HotkeySettingsViewModel>();
        services.AddTransient<AudioSettingsViewModel>();
        services.AddTransient<VoiceSettingsViewModel>();
        services.AddTransient<AppearanceSettingsViewModel>();
        services.AddTransient<ThemeSettingsViewModel>();
        services.AddTransient<PhraseListViewModel>();
        services.AddTransient<PhraseEditorViewModel>();
        services.AddTransient<TextReplacementSettingsViewModel>();
        services.AddTransient<SettingsViewModel>();
    }
}
