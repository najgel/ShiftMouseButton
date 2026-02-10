using System;

namespace ShiftMouseButton;

public static class HotkeyCli
{
    /// <summary>
    /// Looks for a --hotkey override in args.
    /// Supported forms:
    ///   --hotkey Ctrl+Alt+M
    ///   --hotkey=Ctrl+Alt+M
    /// </summary>
    public static bool TryGetHotkeyOverride(string[]? args, out Hotkey hotkey, out string error)
    {
        hotkey = default;
        error = string.Empty;

        if (args == null || args.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i] ?? string.Empty;

            if (arg.StartsWith("--hotkey=", StringComparison.OrdinalIgnoreCase))
            {
                string value = arg["--hotkey=".Length..];
                if (HotkeyParser.TryParse(value, out hotkey, out error))
                {
                    return true;
                }
                return false;
            }

            if (string.Equals(arg, "--hotkey", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    error = "Missing value for --hotkey. Example: --hotkey \"Ctrl+Alt+M\"";
                    return false;
                }

                string value = args[i + 1] ?? string.Empty;
                if (HotkeyParser.TryParse(value, out hotkey, out error))
                {
                    return true;
                }

                return false;
            }
        }

        return false;
    }
}

