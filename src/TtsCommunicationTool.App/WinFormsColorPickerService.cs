using System.Linq;
using System.Windows;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.UI.Views;

namespace TtsCommunicationTool.App;

/// <summary>
/// Color picker implementations:
/// - PickColor: Screen eyedropper (custom overlay)
/// - PickColorWithDialog: Windows system ColorDialog
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

    public string? PickColorWithDialog(string currentHex)
    {
        using var dialog = new System.Windows.Forms.ColorDialog
        {
            FullOpen     = true,
            AnyColor     = true,
            SolidColorOnly = false,
        };

        try
        {
            var c = (System.Windows.Media.Color)
                System.Windows.Media.ColorConverter.ConvertFromString(currentHex);
            dialog.Color = System.Drawing.Color.FromArgb(c.R, c.G, c.B);
        }
        catch { /* leave dialog at default colour */ }

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var c = dialog.Color;
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }
        return null;
    }
}

