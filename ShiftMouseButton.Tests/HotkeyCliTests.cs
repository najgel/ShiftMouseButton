using System;
using ShiftMouseButton;
using Xunit;

namespace ShiftMouseButton.Tests;

public class HotkeyCliTests
{
    [Fact]
    public void TryGetHotkeyOverride_WhenMissing_ReturnsFalseWithoutError()
    {
        Assert.False(HotkeyCli.TryGetHotkeyOverride(Array.Empty<string>(), out _, out var error));
        Assert.True(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void TryGetHotkeyOverride_SupportsSeparateArgumentForm()
    {
        Assert.True(HotkeyCli.TryGetHotkeyOverride(new[] { "--hotkey", "Ctrl+Alt+M" }, out var hotkey, out var error), error);
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }

    [Fact]
    public void TryGetHotkeyOverride_SupportsEqualsForm()
    {
        Assert.True(HotkeyCli.TryGetHotkeyOverride(new[] { "--hotkey=Ctrl+Alt+M" }, out var hotkey, out var error), error);
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }

    [Fact]
    public void TryGetHotkeyOverride_WhenValueMissing_ReturnsFalseWithError()
    {
        Assert.False(HotkeyCli.TryGetHotkeyOverride(new[] { "--hotkey" }, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void TryGetHotkeyOverride_WhenInvalidValue_ReturnsFalseWithError()
    {
        Assert.False(HotkeyCli.TryGetHotkeyOverride(new[] { "--hotkey", "Ctrl+Alt+Nope" }, out _, out var error));
        Assert.Contains("Unrecognized", error, System.StringComparison.OrdinalIgnoreCase);
    }
}

