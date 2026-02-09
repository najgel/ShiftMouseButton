namespace ShiftMouseButton;

/// <summary>
/// Manages whether the application is configured to run at Windows startup.
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Returns true if the application is currently set to run at startup.
    /// </summary>
    bool IsStartupEnabled();

    /// <summary>
    /// Enables or disables running at Windows startup.
    /// </summary>
    /// <param name="enable">True to run at startup, false to remove.</param>
    void SetStartupEnabled(bool enable);
}
