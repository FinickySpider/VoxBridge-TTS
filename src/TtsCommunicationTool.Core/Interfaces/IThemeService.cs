using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Manages theme loading, application, and persistence.
/// Implementations must be UI-thread-safe for <see cref="Apply"/>.
/// </summary>
public interface IThemeService
{
    /// <summary>All available themes: built-in (embedded) + user themes from %AppData%.</summary>
    IReadOnlyList<ThemeSettings> LoadAll();

    /// <summary>
    /// Pushes all colour/value resources from <paramref name="theme"/> into
    /// <c>Application.Current.Resources</c> so DynamicResource bindings update live.
    /// Must be called on the UI thread.
    /// </summary>
    void Apply(ThemeSettings theme);

    /// <summary>Applies the Default Dark theme and persists the choice to config.</summary>
    void ResetToDefault();

    /// <summary>Writes <paramref name="theme"/> as a user theme JSON file.</summary>
    Task SaveAsUserThemeAsync(ThemeSettings theme, string name);

    /// <summary>Deletes a user theme file by name.  No-op if name belongs to a built-in.</summary>
    Task DeleteUserThemeAsync(string name);

    /// <summary>Name of the currently active theme.</summary>
    string ActiveThemeName { get; }

    /// <summary>
    /// Raised after <see cref="ResetToDefault"/> completes so that any open settings UI can
    /// refresh itself to reflect the new theme state.
    /// </summary>
    event EventHandler? ThemeResetToDefault;
}
