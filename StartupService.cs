using Microsoft.Win32;

namespace ShiftMouseButton;

/// <summary>
/// Registry-based implementation of startup run key for current user.
/// </summary>
public class StartupService : IStartupService
{
    private const string DefaultRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string DefaultAppName = "MouseButtonSwitcher";
    private readonly Func<string> _getExecutablePath;
    private readonly string _registryKeyPath;
    private readonly string _appName;

    public StartupService(Func<string> getExecutablePath, string? registryKeyPath = null, string? appName = null)
    {
        _getExecutablePath = getExecutablePath ?? throw new ArgumentNullException(nameof(getExecutablePath));
        _registryKeyPath = registryKeyPath ?? DefaultRegistryKeyPath;
        _appName = appName ?? DefaultAppName;
    }

    /// <inheritdoc />
    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(_registryKeyPath, false);
            if (key != null)
            {
                object? value = key.GetValue(_appName);
                return value != null;
            }
        }
        catch
        {
            // If we can't read the registry, assume it's not enabled
        }

        return false;
    }

    /// <inheritdoc />
    public void SetStartupEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(_registryKeyPath, true);
            if (key != null)
            {
                if (enable)
                {
                    key.SetValue(_appName, _getExecutablePath());
                }
                else
                {
                    key.DeleteValue(_appName, false);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to {(enable ? "enable" : "disable")} startup: {ex.Message}",
                ex);
        }
    }
}
