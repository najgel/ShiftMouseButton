using Microsoft.Win32;
using ShiftMouseButton;
using Xunit;

namespace ShiftMouseButton.Tests;

public class StartupServiceTests : IDisposable
{
    private const string TestKeyPath = @"SOFTWARE\ShiftMouseButton_Tests";
    private const string TestAppName = "TestRun";
    private readonly string _exePath = System.IO.Path.GetFullPath("test.exe");

    public void Dispose()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree("SOFTWARE\\ShiftMouseButton_Tests", false);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void IsStartupEnabled_WhenKeyDoesNotExist_ReturnsFalse()
    {
        EnsureTestKeyExists();
        var service = new StartupService(() => _exePath, TestKeyPath, TestAppName);
        Assert.False(service.IsStartupEnabled());
    }

    [Fact]
    public void IsStartupEnabled_WhenValueExists_ReturnsTrue()
    {
        EnsureTestKeyExists();
        using (var key = Registry.CurrentUser.OpenSubKey(TestKeyPath, true))
        {
            key!.SetValue(TestAppName, _exePath);
        }

        var service = new StartupService(() => _exePath, TestKeyPath, TestAppName);
        Assert.True(service.IsStartupEnabled());
    }

    [Fact]
    public void SetStartupEnabled_True_SetsValue()
    {
        EnsureTestKeyExists();
        var service = new StartupService(() => _exePath, TestKeyPath, TestAppName);

        service.SetStartupEnabled(true);

        Assert.True(service.IsStartupEnabled());
        using (var key = Registry.CurrentUser.OpenSubKey(TestKeyPath, false))
        {
            var value = key?.GetValue(TestAppName);
            Assert.NotNull(value);
            Assert.Equal(_exePath, value.ToString());
        }
    }

    [Fact]
    public void SetStartupEnabled_False_RemovesValue()
    {
        EnsureTestKeyExists();
        using (var key = Registry.CurrentUser.OpenSubKey(TestKeyPath, true))
        {
            key!.SetValue(TestAppName, _exePath);
        }

        var service = new StartupService(() => _exePath, TestKeyPath, TestAppName);
        service.SetStartupEnabled(false);

        Assert.False(service.IsStartupEnabled());
        using (var key = Registry.CurrentUser.OpenSubKey(TestKeyPath, false))
        {
            var value = key?.GetValue(TestAppName);
            Assert.Null(value);
        }
    }

    private static void EnsureTestKeyExists()
    {
        using var key = Registry.CurrentUser.CreateSubKey(TestKeyPath);
        // Key created for test use
    }
}
