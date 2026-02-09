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
}
