namespace ShiftMouseButton;

/// <summary>
/// Helper for mouse button swap state logic (testable without P/Invoke).
/// </summary>
public static class MouseSwapState
{
    /// <summary>
    /// SM_SWAPBUTTON: 0 = not swapped (left primary), non-zero = swapped (right primary).
    /// </summary>
    public static string GetPrimaryButtonName(int swapButtonMetric)
    {
        bool isSwapped = swapButtonMetric != 0;
        return isSwapped ? "Right" : "Left";
    }

    /// <summary>
    /// Returns true if buttons are currently swapped (right is primary).
    /// </summary>
    public static bool IsSwapped(int swapButtonMetric)
    {
        return swapButtonMetric != 0;
    }

    /// <summary>
    /// Returns true if the app should restore the swap state (persisted differs from current).
    /// </summary>
    public static bool ShouldRestoreSwap(bool? persistedSwap, int currentSwapMetric)
    {
        if (persistedSwap is not bool desired)
        {
            return false;
        }

        bool current = IsSwapped(currentSwapMetric);
        return current != desired;
    }
}
