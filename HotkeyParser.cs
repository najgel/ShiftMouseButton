using System;
using System.Linq;

namespace ShiftMouseButton;

public static class HotkeyParser
{
    public static Hotkey MustParse(string text)
    {
        if (!TryParse(text, out var hotkey, out var error))
        {
            throw new FormatException(error);
        }

        return hotkey;
    }

    public static bool TryParse(string? text, out Hotkey hotkey, out string error)
    {
        hotkey = default;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = "Hotkey is empty.";
            return false;
        }

        // Support "Ctrl+Alt+M" (and tolerate whitespace).
        var tokens = text
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        if (tokens.Length == 0)
        {
            error = "Hotkey is empty.";
            return false;
        }

        HotkeyModifiers mods = HotkeyModifiers.None;
        string? keyToken = null;

        foreach (var token in tokens)
        {
            if (IsModifierToken(token, out var mod))
            {
                mods |= mod;
                continue;
            }

            if (keyToken != null)
            {
                error = "Hotkey must be modifiers plus a single key (e.g. Ctrl+Alt+M).";
                return false;
            }

            keyToken = token;
        }

        if (keyToken == null)
        {
            error = "Hotkey must include a non-modifier key (e.g. Ctrl+Alt+M).";
            return false;
        }

        if (!VirtualKeyHelpers.TryParse(keyToken, out var vk))
        {
            error = $"Unrecognized key '{keyToken}'.";
            return false;
        }

        var parsed = new Hotkey(mods, vk);
        if (!parsed.IsValid)
        {
            error = "Invalid hotkey. Choose a non-modifier key.";
            return false;
        }

        hotkey = parsed;
        return true;
    }

    private static bool IsModifierToken(string token, out HotkeyModifiers mod)
    {
        mod = HotkeyModifiers.None;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        switch (token.Trim().ToLowerInvariant())
        {
            case "ctrl":
            case "control":
                mod = HotkeyModifiers.Control;
                return true;
            case "alt":
                mod = HotkeyModifiers.Alt;
                return true;
            case "shift":
                mod = HotkeyModifiers.Shift;
                return true;
            case "win":
            case "windows":
                mod = HotkeyModifiers.Win;
                return true;
            default:
                return false;
        }
    }
}

