using System;
using System.IO;
using ShiftMouseButton;
using Xunit;

namespace ShiftMouseButton.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempFilePath;

    public SettingsServiceTests()
    {
        _tempFilePath = Path.GetTempFileName();
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public void LoadSwapState_WhenFileMissing_ReturnsNull()
    {
        File.Delete(_tempFilePath);
        var service = new JsonSettingsService(_tempFilePath);
        Assert.Null(service.LoadSwapState());
    }

    [Fact]
    public void LoadSwapState_WhenSwapButtonsTrue_ReturnsTrue()
    {
        File.WriteAllText(_tempFilePath, """{"hotkey":"Ctrl+Alt+M","swapButtons":true}""");
        var service = new JsonSettingsService(_tempFilePath);
        Assert.True(service.LoadSwapState());
    }

    [Fact]
    public void LoadSwapState_WhenSwapButtonsFalse_ReturnsFalse()
    {
        File.WriteAllText(_tempFilePath, """{"hotkey":"Ctrl+Alt+M","swapButtons":false}""");
        var service = new JsonSettingsService(_tempFilePath);
        Assert.False(service.LoadSwapState());
    }

    [Fact]
    public void SaveSwapState_WritesToFile()
    {
        var service = new JsonSettingsService(_tempFilePath);
        service.SaveSwapState(true);
        Assert.True(service.LoadSwapState());
    }

    [Fact]
    public void SaveHotkey_PreservesSwapButtons()
    {
        var service = new JsonSettingsService(_tempFilePath);
        service.SaveSwapState(true);
        service.SaveHotkey(Hotkey.Default);
        Assert.True(service.LoadSwapState());
    }

    [Fact]
    public void SaveSwapState_PreservesHotkey()
    {
        var service = new JsonSettingsService(_tempFilePath);
        service.SaveHotkey(Hotkey.Default);
        service.SaveSwapState(true);
        var hotkey = service.LoadHotkey();
        Assert.Equal("Ctrl+Alt+M", hotkey.ToString());
    }
}
