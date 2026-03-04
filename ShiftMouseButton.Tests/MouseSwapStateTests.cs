using ShiftMouseButton;
using Xunit;

namespace ShiftMouseButton.Tests;

public class MouseSwapStateTests
{
    [Theory]
    [InlineData(0, "Left")]
    [InlineData(1, "Right")]
    [InlineData(42, "Right")]
    public void GetPrimaryButtonName_ReturnsCorrectName(int swapButtonMetric, string expected)
    {
        string result = MouseSwapState.GetPrimaryButtonName(swapButtonMetric);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    public void IsSwapped_ReturnsCorrectState(int swapButtonMetric, bool expected)
    {
        bool result = MouseSwapState.IsSwapped(swapButtonMetric);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldRestoreSwap_WhenNoPersisted_ReturnsFalse()
    {
        Assert.False(MouseSwapState.ShouldRestoreSwap(null, 0));
        Assert.False(MouseSwapState.ShouldRestoreSwap(null, 1));
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public void ShouldRestoreSwap_WhenPersistedMatchesCurrent_ReturnsFalse(bool persisted, int currentMetric)
    {
        Assert.False(MouseSwapState.ShouldRestoreSwap(persisted, currentMetric));
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public void ShouldRestoreSwap_WhenPersistedDiffersFromCurrent_ReturnsTrue(bool persisted, int currentMetric)
    {
        Assert.True(MouseSwapState.ShouldRestoreSwap(persisted, currentMetric));
    }
}
