namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Presents a native color-picker dialog and returns the chosen hex string,
/// or null if the user cancelled.
/// </summary>
public interface IColorPickerService
{
    /// <summary>
    /// Shows a modal color picker pre-seeded with <paramref name="currentHex"/>.
    /// Returns the selected colour as a 7-character hex string (e.g. "#CBA6F7")
    /// or <c>null</c> if the user dismissed the dialog without choosing.
    /// </summary>
    string? PickColor(string currentHex);
}
