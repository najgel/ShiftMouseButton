# Builds the ShiftMouseButton MSI using Release output (not Debug).
# Run from the repository root.
# Prerequisites: .NET SDK, WiX (via NuGet when building the installer).

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$PublishDir = Join-Path $ProjectRoot "ShiftMouseButton.Installer\publish"

Write-Host "Publishing app (Release, win-x64) to installer folder..."
dotnet publish (Join-Path $ProjectRoot "ShiftMouseButton.csproj") -c Release -r win-x64 -o $PublishDir

if (-not (Test-Path (Join-Path $PublishDir "ShiftMouseButton.exe"))) {
    throw "Publish did not produce ShiftMouseButton.exe in $PublishDir"
}

Write-Host "Building installer..."
dotnet build (Join-Path $ProjectRoot "ShiftMouseButton.Installer\ShiftMouseButton.Installer.wixproj") -c Release

$Msi = Join-Path $ProjectRoot "ShiftMouseButton.Installer\bin\Release\ShiftMouseButton.msi"
Write-Host "Done. MSI: $Msi"
Write-Host "Install the MSI, then check the 'Run ShiftMouseButton at Windows startup' box and click Finish to add the registry key."
