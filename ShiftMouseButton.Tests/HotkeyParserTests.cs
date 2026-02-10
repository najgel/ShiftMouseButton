using ShiftMouseButton;
using Xunit;

namespace ShiftMouseButton.Tests;

public class HotkeyParserTests
{
    [Fact]
    public void TryParse_CtrlAltM_ParsesCorrectly()
    {
        Assert.True(HotkeyParser.TryParse("Ctrl+Alt+M", out var hotkey, out var error), error);
        Assert.Equal(HotkeyModifiers.Control | HotkeyModifiers.Alt, hotkey.Modifiers);
        Assert.Equal((uint)'M', hotkey.VirtualKey);
        Assert.True(hotkey.IsValid);
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }

    [Fact]
    public void TryParse_IsCaseInsensitiveAndNormalizesOrder()
    {
        Assert.True(HotkeyParser.TryParse("alt+ctrl+m", out var hotkey, out var error), error);
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }

    [Fact]
    public void TryParse_ModifierOnly_Fails()
    {
        Assert.False(HotkeyParser.TryParse("Ctrl+Alt", out _, out var error));
        Assert.Contains("include", error, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryParse_UnknownKey_Fails()
    {
        Assert.False(HotkeyParser.TryParse("Ctrl+Alt+Nope", out _, out var error));
        Assert.Contains("Unrecognized", error, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryParse_HexVirtualKey_ParsesCorrectly()
    {
        Assert.True(HotkeyParser.TryParse("Ctrl+Alt+0x4D", out var hotkey, out var error), error);
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }
}

