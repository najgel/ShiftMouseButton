# ShiftMouseButton (Mouse Button Switcher)

A small Windows tray utility that lets you swap the primary mouse button (left/right) with a hotkey—handy for left-handed use or temporary swap.

**Website:** [najgel.github.io/ShiftMouseButton](https://najgel.github.io/ShiftMouseButton)

## Features

- **Hotkey support**: Press `Ctrl+Alt+M` (default) to swap mouse buttons (global, works from any app) — configurable
- **System tray**: Runs in the background with a tray icon
- **Notifications**: Balloon tip when buttons are swapped
- **Context menu**: Right-click the tray icon to swap, toggle startup, or exit
- **Start at login**: Optional "Run at Startup" to start with Windows

## Requirements

- Windows
- [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (if not using self-contained build)

## Install

- **MSI (recommended):** Download `ShiftMouseButton-x.x.x.msi` from [Releases](https://github.com/najgel/ShiftMouseButton/releases), run the installer. Requires .NET 8 Desktop Runtime if not bundled.
- **Portable:** Download the ZIP from Releases, extract, and run `ShiftMouseButton.exe`. Use "Run at Startup" from the tray if you want it to start with Windows.

## Build and run

```bash
dotnet build
dotnet run
```

Or run the executable from `bin/Debug/net8.0-windows/ShiftMouseButton.exe` (or `Release` after a release build).

## Usage

1. **Swap mouse buttons**
   - Press your configured hotkey (default: `Ctrl+Alt+M`)
   - Or left-click the system tray icon
   - Or right-click the tray icon and choose "Swap Mouse Buttons (…)"

2. **Change hotkey**
   - Right-click the tray icon → "Hotkey..."
   - Settings are saved in `%AppData%\ShiftMouseButton\settings.json`
   - Optional command-line override: `ShiftMouseButton.exe --hotkey "Ctrl+Alt+M"`

3. **Run at startup**
   - Right-click the tray icon → "Run at Startup" (checkmark when enabled)

4. **Exit**
   - Right-click the tray icon → "Exit"

## How it works

The app uses the Windows API:

- `SwapMouseButton()` – swaps the button configuration
- `RegisterHotKey()` – registers the global hotkey
- `GetSystemMetrics(SM_SWAPBUTTON)` – reads the current swap state

## Development

- **Run tests:** `dotnet test`
- **GitHub Pages:** The `docs/` folder contains the project website. Enable in repo **Settings → Pages → Deploy from a branch** (branch: `master`, folder: `/docs`).
- **Release build:** `dotnet publish -c Release` (see Build/release below for options)
- **Build MSI:** Use **Release** so the installed app is not a debug build. From the repo root either:
  - **Recommended:** run the script:
    ```powershell
    .\build-installer.ps1
    ```
  - Or run these two commands (must use `-c Release`):
    ```bash
    dotnet publish ShiftMouseButton.csproj -c Release -r win-x64 -o ShiftMouseButton.Installer/publish
    dotnet build ShiftMouseButton.Installer/ShiftMouseButton.Installer.wixproj -c Release
    ```
  The MSI is produced in `ShiftMouseButton.Installer/bin/Release/`. On the installer’s last screen, check **Run ShiftMouseButton at Windows startup** and click Finish to add the app to Windows startup (same as the in-app “Run at Startup” option). Requires [WiX Toolset](https://wixtoolset.org/) (SDK-style build via NuGet).

## Build and release

- **Framework-dependent:** `dotnet publish -c Release -f net8.0-windows` – output in `bin/Release/net8.0-windows/publish/`. Users need .NET 8 Desktop Runtime.
- **Single-file (portable):** Add `-p:PublishSingleFile=true` for one .exe; document in Releases that the runtime may be included.
- **CI:** The repo includes a GitHub Actions workflow to build and test on push/PR, and to produce artifacts on release.

## License

Licensed under the [MIT License](LICENSE).
