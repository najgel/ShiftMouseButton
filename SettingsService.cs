using System;
using System.IO;
using System.Text.Json;

namespace ShiftMouseButton;

public interface ISettingsService
{
    Hotkey LoadHotkey();
    void SaveHotkey(Hotkey hotkey);
    string SettingsFilePath { get; }
}

internal sealed class JsonSettingsService : ISettingsService
{
    private readonly string _settingsFilePath;

    public JsonSettingsService(string? settingsFilePath = null)
    {
        _settingsFilePath = settingsFilePath ?? GetDefaultSettingsFilePath();
    }

    public string SettingsFilePath => _settingsFilePath;

    public Hotkey LoadHotkey()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return Hotkey.Default;
            }

            string json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

            if (settings?.Hotkey is { Length: > 0 } hotkeyText &&
                HotkeyParser.TryParse(hotkeyText, out var hotkey, out _))
            {
                return hotkey;
            }
        }
        catch
        {
            // ignore and fall back to default
        }

        return Hotkey.Default;
    }

    public void SaveHotkey(Hotkey hotkey)
    {
        if (!hotkey.IsValid)
        {
            throw new ArgumentException("Hotkey is invalid.", nameof(hotkey));
        }

        string? dir = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var settings = new AppSettings { Hotkey = hotkey.ToString() };
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_settingsFilePath, json);
    }

    private static string GetDefaultSettingsFilePath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "ShiftMouseButton", "settings.json");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    private sealed class AppSettings
    {
        public string? Hotkey { get; set; }
    }
}

