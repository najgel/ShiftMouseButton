using System;

namespace ShiftMouseButton;

[Flags]
public enum HotkeyModifiers : uint
{
    None = 0,
    Alt = 0x0001,
    Control = 0x0002,
    Shift = 0x0004,
    Win = 0x0008,
}

/// <summary>
/// Represents a global hotkey compatible with Win32 RegisterHotKey:
/// modifier flags + a single virtual-key (VK_*) value.
/// </summary>
public readonly record struct Hotkey(HotkeyModifiers Modifiers, uint VirtualKey)
{
    public static Hotkey Default => HotkeyParser.MustParse("Ctrl+Alt+M");

    public bool IsValid => VirtualKey != 0 && !VirtualKeyHelpers.IsModifierVirtualKey(VirtualKey);

    public override string ToString() => HotkeyFormatter.Format(this);
}

internal static class HotkeyFormatter
{
    public static string Format(Hotkey hotkey)
    {
        if (!hotkey.IsValid)
        {
            return "(invalid)";
        }

        var parts = new System.Collections.Generic.List<string>(5);

        if (hotkey.Modifiers.HasFlag(HotkeyModifiers.Control)) parts.Add("Ctrl");
        if (hotkey.Modifiers.HasFlag(HotkeyModifiers.Alt)) parts.Add("Alt");
        if (hotkey.Modifiers.HasFlag(HotkeyModifiers.Shift)) parts.Add("Shift");
        if (hotkey.Modifiers.HasFlag(HotkeyModifiers.Win)) parts.Add("Win");

        parts.Add(VirtualKeyHelpers.Format(hotkey.VirtualKey));
        return string.Join("+", parts);
    }
}

internal static class VirtualKeyHelpers
{
    // Common modifier virtual keys
    private const uint VK_SHIFT = 0x10;
    private const uint VK_CONTROL = 0x11;
    private const uint VK_MENU = 0x12; // Alt
    private const uint VK_LWIN = 0x5B;
    private const uint VK_RWIN = 0x5C;

    public static bool IsModifierVirtualKey(uint vk) =>
        vk == VK_SHIFT ||
        vk == VK_CONTROL ||
        vk == VK_MENU ||
        vk == VK_LWIN ||
        vk == VK_RWIN;

    public static string Format(uint vk)
    {
        // A-Z
        if (vk is >= 0x41 and <= 0x5A)
        {
            return ((char)vk).ToString();
        }

        // 0-9
        if (vk is >= 0x30 and <= 0x39)
        {
            return ((char)vk).ToString();
        }

        // F1-F24
        if (vk is >= 0x70 and <= 0x87)
        {
            return $"F{(vk - 0x70) + 1}";
        }

        return vk switch
        {
            0x1B => "Esc",
            0x09 => "Tab",
            0x0D => "Enter",
            0x20 => "Space",
            0x08 => "Backspace",
            0x2E => "Delete",
            0x2D => "Insert",
            0x24 => "Home",
            0x23 => "End",
            0x21 => "PageUp",
            0x22 => "PageDown",
            0x25 => "Left",
            0x26 => "Up",
            0x27 => "Right",
            0x28 => "Down",
            _ => $"VK_0x{vk:X2}",
        };
    }

    public static bool TryParse(string token, out uint vk)
    {
        vk = 0;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        token = token.Trim();

        // 0xNN hex
        if (token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
            uint.TryParse(token.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out vk))
        {
            return vk != 0;
        }

        // Single letter or digit
        if (token.Length == 1)
        {
            char c = token[0];
            if (c is >= 'a' and <= 'z') c = (char)(c - 32);
            if (c is >= 'A' and <= 'Z')
            {
                vk = c;
                return true;
            }

            if (c is >= '0' and <= '9')
            {
                vk = c;
                return true;
            }
        }

        // Function keys (F1-F24)
        if ((token.Length is 2 or 3) &&
            (token[0] == 'F' || token[0] == 'f') &&
            int.TryParse(token.AsSpan(1), out int fNum) &&
            fNum is >= 1 and <= 24)
        {
            vk = (uint)(0x70 + (fNum - 1));
            return true;
        }

        // Named keys
        switch (token.ToLowerInvariant())
        {
            case "esc":
            case "escape":
                vk = 0x1B; return true;
            case "tab":
                vk = 0x09; return true;
            case "enter":
            case "return":
                vk = 0x0D; return true;
            case "space":
            case "spacebar":
                vk = 0x20; return true;
            case "backspace":
            case "bksp":
                vk = 0x08; return true;
            case "delete":
            case "del":
                vk = 0x2E; return true;
            case "insert":
            case "ins":
                vk = 0x2D; return true;
            case "home":
                vk = 0x24; return true;
            case "end":
                vk = 0x23; return true;
            case "pageup":
            case "pgup":
                vk = 0x21; return true;
            case "pagedown":
            case "pgdn":
                vk = 0x22; return true;
            case "left":
                vk = 0x25; return true;
            case "up":
                vk = 0x26; return true;
            case "right":
                vk = 0x27; return true;
            case "down":
                vk = 0x28; return true;
        }

        return false;
    }
}

