# Blindfold Chess Installation Script for Windows (PowerShell)

$ErrorActionPreference = 'Stop'

$REPO = "bertt/blindfoldchess"
$INSTALL_DIR = "$env:LOCALAPPDATA\blindfoldchess"
$BINARY_NAME = "blindfoldchess.exe"

Write-Host "╔════════════════════════════════════════════╗"
Write-Host "║   Blindfold Chess - Installation Script   ║"
Write-Host "╚════════════════════════════════════════════╝"
Write-Host ""

# Detect architecture
$ARCH = if ([System.Environment]::Is64BitOperatingSystem) {
    if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq 'Arm64') {
        "arm64"
    } else {
        "x64"
    }
} else {
    Write-Host "❌ Error: 32-bit Windows is not supported"
    exit 1
}

Write-Host "📦 Detected platform: windows-$ARCH"
Write-Host ""

# Get latest release info
Write-Host "🔍 Fetching latest release..."
try {
    $releaseInfo = Invoke-RestMethod -Uri "https://api.github.com/repos/$REPO/releases/latest"
    $LATEST_VERSION = $releaseInfo.tag_name
} catch {
    Write-Host "❌ Error: Could not fetch latest release version"
    exit 1
}

Write-Host "📌 Latest version: $LATEST_VERSION"
Write-Host ""

# Construct download URL
$ASSET_NAME = "blindfoldchess-windows-$ARCH.zip"
$DOWNLOAD_URL = "https://github.com/$REPO/releases/download/$LATEST_VERSION/$ASSET_NAME"

Write-Host "⬇️  Downloading $ASSET_NAME..."
$TEMP_DIR = [System.IO.Path]::GetTempPath() + [System.Guid]::NewGuid().ToString()
New-Item -ItemType Directory -Path $TEMP_DIR | Out-Null
$TEMP_ZIP = Join-Path $TEMP_DIR "$ASSET_NAME"

try {
    Invoke-WebRequest -Uri $DOWNLOAD_URL -OutFile $TEMP_ZIP
} catch {
    Write-Host "❌ Error: Failed to download release"
    Remove-Item -Recurse -Force $TEMP_DIR
    exit 1
}

Write-Host "📦 Extracting..."
Expand-Archive -Path $TEMP_ZIP -DestinationPath $TEMP_DIR -Force

# Create install directory if it doesn't exist
if (-not (Test-Path $INSTALL_DIR)) {
    New-Item -ItemType Directory -Path $INSTALL_DIR | Out-Null
}

# Find and install binary
Write-Host "📥 Installing to $INSTALL_DIR..."
$binaryPath = Get-ChildItem -Path $TEMP_DIR -Filter $BINARY_NAME -Recurse | Select-Object -First 1

if ($null -eq $binaryPath) {
    Write-Host "❌ Error: Could not find binary in archive"
    Remove-Item -Recurse -Force $TEMP_DIR
    exit 1
}

Copy-Item -Path $binaryPath.FullName -Destination (Join-Path $INSTALL_DIR $BINARY_NAME) -Force

# Cleanup
Remove-Item -Recurse -Force $TEMP_DIR

Write-Host ""
Write-Host "✅ Installation complete!"
Write-Host ""
Write-Host "Installed to: $INSTALL_DIR\$BINARY_NAME"
Write-Host "Version: $LATEST_VERSION"
Write-Host ""

# Check if directory is in PATH
$currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
if ($currentPath -notlike "*$INSTALL_DIR*") {
    Write-Host "⚠️  Adding to PATH..."
    try {
        $newPath = "$INSTALL_DIR;$currentPath"
        [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
        Write-Host "✅ Added to PATH (restart terminal to take effect)"
        Write-Host ""
        Write-Host "For immediate use in this session, run:"
        Write-Host "   `$env:Path = `"$INSTALL_DIR;`$env:Path`""
        Write-Host "   $BINARY_NAME"
    } catch {
        Write-Host "⚠️  Could not automatically add to PATH"
        Write-Host ""
        Write-Host "Please add manually: $INSTALL_DIR"
        Write-Host "Or run directly: $INSTALL_DIR\$BINARY_NAME"
    }
} else {
    Write-Host "🚀 Run with: $BINARY_NAME (restart terminal if just installed)"
}

Write-Host ""
Write-Host "📚 GitHub Copilot CLI is also required:"
Write-Host "   npm install -g copilot"
Write-Host ""
