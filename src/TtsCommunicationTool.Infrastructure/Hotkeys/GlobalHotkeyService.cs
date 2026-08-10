using System.Runtime.InteropServices;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Hotkeys;

public sealed class GlobalHotkeyService : IHotkeyService
{
    private readonly Dictionary<int, string> _registeredIds = new();
    private readonly nint _hwnd;
    private int _nextAtomId = 0xC000;
    private bool _disposed;

    public event EventHandler<string>? HotkeyPressed;

    public GlobalHotkeyService(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public OperationResult Register(string id, HotkeyBinding binding)
    {
        var modifiers = GetModifiers(binding);
        var vk = KeyToVk(binding.Key);
        if (vk == 0)
            return OperationResult.Fail($"Unknown key: {binding.Key}");

        var atomId = _nextAtomId++;
        if (!RegisterHotKey(_hwnd, atomId, modifiers, vk))
        {
            var error = Marshal.GetLastWin32Error();
            return OperationResult.Fail($"Failed to register hotkey '{binding}'. Win32 error: {error}");
        }

        _registeredIds[atomId] = id;
        return OperationResult.Ok();
    }

    public void Unregister(string id)
    {
        var entry = _registeredIds.FirstOrDefault(kv => kv.Value == id);
        if (entry.Value is not null)
        {
            UnregisterHotKey(_hwnd, entry.Key);
            _registeredIds.Remove(entry.Key);
        }
    }

    public void UnregisterAll()
    {
        foreach (var key in _registeredIds.Keys.ToList())
        {
            UnregisterHotKey(_hwnd, key);
        }
        _registeredIds.Clear();
    }

    public void ProcessHotkeyMessage(int atomId)
    {
        if (_registeredIds.TryGetValue(atomId, out var id))
        {
            HotkeyPressed?.Invoke(this, id);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        UnregisterAll();
    }

    private static uint GetModifiers(HotkeyBinding b)
    {
        uint mod = 0;
        if (b.Alt) mod |= MOD_ALT;
        if (b.Ctrl) mod |= MOD_CONTROL;
        if (b.Shift) mod |= MOD_SHIFT;
        if (b.Win) mod |= MOD_WIN;
        mod |= MOD_NOREPEAT;
        return mod;
    }

    private static uint KeyToVk(string key)
    {
        // Map common key names to virtual key codes
        // WPF Key enum ToString() values and common aliases
        return key.ToUpperInvariant() switch
        {
            "SPACE" => 0x20,
            "BACK" or "BACKSPACE" => 0x08,
            "RETURN" or "ENTER" => 0x0D,
            "ESCAPE" or "ESC" => 0x1B,
            "TAB" => 0x09,
            "DELETE" or "DEL" => 0x2E,
            "INSERT" or "INS" => 0x2D,
            "HOME" => 0x24,
            "END" => 0x23,
            "PAGEUP" or "PGUP" or "PRIOR" => 0x21,
            "PAGEDOWN" or "PGDN" or "NEXT" => 0x22,
            "UP" => 0x26,
            "DOWN" => 0x28,
            "LEFT" => 0x25,
            "RIGHT" => 0x27,
            "F1" => 0x70, "F2" => 0x71, "F3" => 0x72, "F4" => 0x73,
            "F5" => 0x74, "F6" => 0x75, "F7" => 0x76, "F8" => 0x77,
            "F9" => 0x78, "F10" => 0x79, "F11" => 0x7A, "F12" => 0x7B,
            "F13" => 0x7C, "F14" => 0x7D, "F15" => 0x7E, "F16" => 0x7F,
            "F17" => 0x80, "F18" => 0x81, "F19" => 0x82, "F20" => 0x83,
            "F21" => 0x84, "F22" => 0x85, "F23" => 0x86, "F24" => 0x87,
            // Number keys (WPF Key.D0-D9 ToString gives "D0"-"D9")
            "D0" => 0x30, "D1" => 0x31, "D2" => 0x32, "D3" => 0x33,
            "D4" => 0x34, "D5" => 0x35, "D6" => 0x36, "D7" => 0x37,
            "D8" => 0x38, "D9" => 0x39,
            // NumPad keys (WPF Key.NumPadX ToString)
            "NUMPAD0" => 0x60, "NUMPAD1" => 0x61, "NUMPAD2" => 0x62,
            "NUMPAD3" => 0x63, "NUMPAD4" => 0x64, "NUMPAD5" => 0x65,
            "NUMPAD6" => 0x66, "NUMPAD7" => 0x67, "NUMPAD8" => 0x68,
            "NUMPAD9" => 0x69,
            "MULTIPLY" => 0x6A, "ADD" => 0x6B, "SUBTRACT" => 0x6D,
            "DECIMAL" => 0x6E, "DIVIDE" => 0x6F,
            // OEM keys (WPF Key.OemX ToString values AND numbered aliases Key.Oem1/3/5/6/7)
            "OEM_3" or "OEMTILDE" or "OEM3" or "`" => 0xC0,
            "OEMMINUS" or "OEM_MINUS" => 0xBD,
            "OEMPLUS" or "OEM_PLUS" => 0xBB,
            "OEMOPENBRACKETS" or "OEM4" => 0xDB,
            "OEM6" or "OEMCLOSEBRACKETS" => 0xDD,
            "OEM5" or "OEMPIPE" => 0xDC,
            "OEM1" or "OEMSEMICOLON" => 0xBA,
            "OEMQUOTES" or "OEM7" => 0xDE,
            "OEMCOMMA" or "OEM8" => 0xBC,
            "OEMPERIOD" => 0xBE,
            "OEMQUESTION" or "OEM2" => 0xBF,
            // Caps/Num/Scroll lock
            "CAPITAL" or "CAPSLOCK" => 0x14,
            "NUMLOCK" => 0x90,
            "SCROLL" or "SCROLLLOCK" => 0x91,
            // Single printable char (A-Z, 0-9)
            var k when k.Length == 1 && char.IsLetterOrDigit(k[0]) => (uint)char.ToUpper(k[0]),
            _ => 0
        };
    }

    // P/Invoke
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_WIN = 0x0008;
    private const uint MOD_NOREPEAT = 0x4000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
