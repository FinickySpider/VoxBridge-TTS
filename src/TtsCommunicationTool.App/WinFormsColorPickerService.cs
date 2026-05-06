using System.Linq;
using System.Windows;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.UI.Views;

namespace TtsCommunicationTool.App;

/// <summary>
/// Screen eyedropper color picker.
/// Shows a full-screen overlay of the current desktop; the user hovers to preview, clicks to pick.
/// All open application windows are briefly hidden so the desktop and other apps are fully visible.
/// </summary>
public sealed class WinFormsColorPickerService : IColorPickerService
{
    public string? PickColor(string currentHex)
    {
        // Hide all open app windows so the screenshot shows what's behind them
        var visible = System.Windows.Application.Current.Windows
                                  .Cast<Window>()
                                  .Where(w => w.IsVisible)
                                  .ToList();
        foreach (var w in visible)
            w.Hide();

        string? result = null;
        try
        {
            var picker = new ScreenColorPickerWindow();
            picker.ShowDialog();
            result = picker.PickedColor;
        }
        finally
        {
            // Always restore windows, even if picker threw
            foreach (var w in visible)
                w.Show();
        }

        return result;
    }
}

