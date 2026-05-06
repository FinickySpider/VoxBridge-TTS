namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Presents a native color-picker dialog and returns the chosen hex string,
/// or null if the user cancelled.
/// </summary>
public interface IColorPickerService
{
    /// <summary>
    /// Shows the screen eyedropper overlay pre-seeded with <paramref name="currentHex"/>.
    /// Returns the selected colour as a 7-character hex string or <c>null</c> if cancelled.
    /// </summary>
    string? PickColor(string currentHex);

    /// <summary>
    /// Shows the Windows system colour-chooser dialog pre-seeded with <paramref name="currentHex"/>.
    /// Returns the selected colour as a 7-character hex string or <c>null</c> if cancelled.
    /// </summary>
    string? PickColorWithDialog(string currentHex);
}
