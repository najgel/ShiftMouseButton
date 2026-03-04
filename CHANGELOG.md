# Changelog

## [1.0.2] - 2026-03-04

### Fixed
- **Persist mouse swap state across reboots** – Swap preference is now saved and restored on startup

## [1.0.1] - 2026-02-19

### Fixed
- Release pipeline – asset upload now succeeds (permission fix, modernized upload action)

## [1.0.0] - 2025-02-10

### Added
- **Configurable hotkey** – Change the global hotkey from the tray menu (Hotkey…)
- Settings stored in `%AppData%\ShiftMouseButton\settings.json`
- Command-line override: `--hotkey "Ctrl+Alt+M"` or `--hotkey=Ctrl+Alt+M`

### Features
- Global hotkey (default Ctrl+Alt+M) to swap primary mouse button
- System tray icon with context menu
- Run at Windows startup option
- MSI installer and portable ZIP builds
