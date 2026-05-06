using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TtsCommunicationTool.UI.Views;

/// <summary>
/// Full-screen eyedropper overlay.
/// Captures the desktop on open, lets the user hover to preview and click to pick any pixel.
/// Caller: show with <c>ShowDialog()</c>, then read <see cref="PickedColor"/>.
/// </summary>
public partial class ScreenColorPickerWindow : Window
{
    // ── Win32 ─────────────────────────────────────────────────────────────────
    [DllImport("user32.dll")] static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] static extern int    ReleaseDC(IntPtr hwnd, IntPtr hdc);
    [DllImport("gdi32.dll")]  static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int w, int h);
    [DllImport("gdi32.dll")]  static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdi);
    [DllImport("gdi32.dll")]  static extern bool   BitBlt(IntPtr hdcD, int xD, int yD, int w, int h,
                                                          IntPtr hdcS, int xS, int yS, uint rop);
    [DllImport("gdi32.dll")]  static extern bool   DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")]  static extern bool   DeleteObject(IntPtr hobj);
    const uint SRCCOPY = 0x00CC0020;

    // ── State ─────────────────────────────────────────────────────────────────
    private BitmapSource? _screenshot;
    private byte[]?       _pixels;      // pre-decoded Bgr32 pixel array for fast sampling
    private int           _stride;
    private int           _vW, _vH;    // virtual screen size in physical pixels
    private double        _dpiX = 1.0, _dpiY = 1.0;

    /// <summary>The hex colour chosen by the user (e.g. <c>#A1B2C3</c>), or <c>null</c> if cancelled.</summary>
    public string? PickedColor { get; private set; }

    // ── Constructor ───────────────────────────────────────────────────────────
    public ScreenColorPickerWindow()
    {
        InitializeComponent();
        Cursor = Cursors.Cross;
    }

    // ── Initialise ────────────────────────────────────────────────────────────
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Resolve per-monitor DPI
        var ps = PresentationSource.FromVisual(this);
        if (ps?.CompositionTarget is { } ct)
        {
            _dpiX = ct.TransformToDevice.M11;
            _dpiY = ct.TransformToDevice.M22;
        }

        // Cover the entire virtual screen (all monitors)
        Left   = SystemParameters.VirtualScreenLeft;
        Top    = SystemParameters.VirtualScreenTop;
        Width  = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        // Physical-pixel dimensions of the virtual screen
        _vW = (int)(SystemParameters.VirtualScreenWidth  * _dpiX);
        _vH = (int)(SystemParameters.VirtualScreenHeight * _dpiY);

        CaptureDesktop();
        Focus();
    }

    // ── Screen capture ────────────────────────────────────────────────────────
    private void CaptureDesktop()
    {
        int vLeft = (int)(SystemParameters.VirtualScreenLeft * _dpiX);
        int vTop  = (int)(SystemParameters.VirtualScreenTop  * _dpiY);

        var hdcSrc = GetDC(GetDesktopWindow());
        var hdcMem = CreateCompatibleDC(hdcSrc);
        var hBmp   = CreateCompatibleBitmap(hdcSrc, _vW, _vH);
        var hOld   = SelectObject(hdcMem, hBmp);

        BitBlt(hdcMem, 0, 0, _vW, _vH, hdcSrc, vLeft, vTop, SRCCOPY);

        SelectObject(hdcMem, hOld);
        DeleteDC(hdcMem);
        ReleaseDC(GetDesktopWindow(), hdcSrc);

        _screenshot = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
            hBmp, IntPtr.Zero, Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        _screenshot.Freeze();
        DeleteObject(hBmp);

        // Pre-decode to Bgr32 byte array for O(1) pixel sampling during mouse move
        var bgr    = new FormatConvertedBitmap(_screenshot, PixelFormats.Bgr32, null, 0);
        _stride    = (_vW * 32 + 7) / 8;
        _pixels    = new byte[(long)_vH * _stride];
        bgr.CopyPixels(_pixels, _stride, 0);

        ScreenImage.Source = _screenshot;
    }

    // ── Mouse handling ────────────────────────────────────────────────────────
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_pixels is null) return;

        var pos = e.GetPosition(this);

        // WPF DIPs → physical pixels
        int px = Math.Clamp((int)(pos.X * _dpiX), 0, _vW - 1);
        int py = Math.Clamp((int)(pos.Y * _dpiY), 0, _vH - 1);

        var (r, g, b) = SampleXY(px, py);
        var hex = $"#{r:X2}{g:X2}{b:X2}";

        // Update HUD
        HexLabel.Text    = hex;
        ColorPreview.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));

        // Magnifier: 17×17 physical pixels → 136×136 display
        UpdateMagnifier(px, py);

        // Keep HUD panel on screen
        const double pad  = 20;
        double panW = MagnifierPanel.ActualWidth  > 0 ? MagnifierPanel.ActualWidth  : 165;
        double panH = MagnifierPanel.ActualHeight > 0 ? MagnifierPanel.ActualHeight : 200;

        double left = pos.X + pad;
        double top  = pos.Y + pad;
        if (left + panW > ActualWidth)  left = pos.X - panW - pad;
        if (top  + panH > ActualHeight) top  = pos.Y - panH - pad;

        System.Windows.Controls.Canvas.SetLeft(MagnifierPanel, left);
        System.Windows.Controls.Canvas.SetTop(MagnifierPanel,  top);
    }

    private void UpdateMagnifier(int cx, int cy)
    {
        if (_screenshot is null) return;

        const int half = 8;  // sample a 17×17 region
        int x0 = Math.Max(0,  cx - half);
        int y0 = Math.Max(0,  cy - half);
        int x1 = Math.Min(_vW, cx + half + 1);
        int y1 = Math.Min(_vH, cy + half + 1);

        var crop = new CroppedBitmap(_screenshot, new Int32Rect(x0, y0, x1 - x0, y1 - y0));
        MagnifierImage.Source = crop;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        if (_pixels is null) return;

        var pos = e.GetPosition(this);
        int px  = Math.Clamp((int)(pos.X * _dpiX), 0, _vW - 1);
        int py  = Math.Clamp((int)(pos.Y * _dpiY), 0, _vH - 1);

        var (r, g, b) = SampleXY(px, py);
        PickedColor = $"#{r:X2}{g:X2}{b:X2}";

        e.Handled = true;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (byte R, byte G, byte B) SampleXY(int x, int y)
    {
        if (_pixels is null) return (128, 128, 128);
        int i = y * _stride + x * 4;
        return (_pixels[i + 2], _pixels[i + 1], _pixels[i]); // Bgr32 stores B,G,R
    }
}
