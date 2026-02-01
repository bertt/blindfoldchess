#!/bin/bash
# Blindfold Chess Installation Script for Linux/macOS

set -e

REPO="bertt/blindfoldchess"
INSTALL_DIR="$HOME/.local/bin"
BINARY_NAME="blindfoldchess"

echo "╔════════════════════════════════════════════╗"
echo "║   Blindfold Chess - Installation Script   ║"
echo "╚════════════════════════════════════════════╝"
echo ""

# Detect OS and architecture
OS=$(uname -s | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

case "$OS" in
    linux*)
        PLATFORM="linux"
        ;;
    darwin*)
        PLATFORM="macos"
        ;;
    *)
        echo "❌ Error: Unsupported operating system: $OS"
        exit 1
        ;;
esac

case "$ARCH" in
    x86_64|amd64)
        ARCH_TYPE="x64"
        ;;
    aarch64|arm64)
        ARCH_TYPE="arm64"
        ;;
    *)
        echo "❌ Error: Unsupported architecture: $ARCH"
        exit 1
        ;;
esac

echo "📦 Detected platform: $PLATFORM-$ARCH_TYPE"
echo ""

# Get latest release info
echo "🔍 Fetching latest release..."
RELEASE_URL="https://api.github.com/repos/$REPO/releases/latest"
LATEST_VERSION=$(curl -s $RELEASE_URL | grep '"tag_name"' | sed -E 's/.*"([^"]+)".*/\1/')

if [ -z "$LATEST_VERSION" ]; then
    echo "❌ Error: Could not fetch latest release version"
    exit 1
fi

echo "📌 Latest version: $LATEST_VERSION"
echo ""

# Construct download URL
ASSET_NAME="blindfoldchess-$PLATFORM-$ARCH_TYPE.zip"
DOWNLOAD_URL="https://github.com/$REPO/releases/download/$LATEST_VERSION/$ASSET_NAME"

echo "⬇️  Downloading $ASSET_NAME..."
TEMP_DIR=$(mktemp -d)
cd "$TEMP_DIR"

if ! curl -L -o "$ASSET_NAME" "$DOWNLOAD_URL"; then
    echo "❌ Error: Failed to download release"
    rm -rf "$TEMP_DIR"
    exit 1
fi

echo "📦 Extracting..."
unzip -q "$ASSET_NAME"

# Create install directory if it doesn't exist
mkdir -p "$INSTALL_DIR"

# Install binary
echo "📥 Installing to $INSTALL_DIR..."
if [ "$PLATFORM" = "macos" ]; then
    # On macOS, the executable might be in a subdirectory
    if [ -f "$BINARY_NAME" ]; then
        cp "$BINARY_NAME" "$INSTALL_DIR/"
    elif [ -f "blindfoldchess" ]; then
        cp "blindfoldchess" "$INSTALL_DIR/"
    else
        # Find the binary
        BINARY=$(find . -name "blindfoldchess" -type f | head -n 1)
        if [ -n "$BINARY" ]; then
            cp "$BINARY" "$INSTALL_DIR/"
        else
            echo "❌ Error: Could not find binary in archive"
            rm -rf "$TEMP_DIR"
            exit 1
        fi
    fi
else
    # Linux
    if [ -f "$BINARY_NAME" ]; then
        cp "$BINARY_NAME" "$INSTALL_DIR/"
    elif [ -f "blindfoldchess" ]; then
        cp "blindfoldchess" "$INSTALL_DIR/"
    else
        BINARY=$(find . -name "blindfoldchess" -type f | head -n 1)
        if [ -n "$BINARY" ]; then
            cp "$BINARY" "$INSTALL_DIR/"
        else
            echo "❌ Error: Could not find binary in archive"
            rm -rf "$TEMP_DIR"
            exit 1
        fi
    fi
fi

chmod +x "$INSTALL_DIR/$BINARY_NAME"

# Cleanup
cd - > /dev/null
rm -rf "$TEMP_DIR"

echo ""
echo "✅ Installation complete!"
echo ""
echo "Installed to: $INSTALL_DIR/$BINARY_NAME"
echo "Version: $LATEST_VERSION"
echo ""

# Check if directory is in PATH
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
    echo "⚠️  Note: $INSTALL_DIR is not in your PATH"
    echo ""
    echo "To add it, add this line to your shell config (~/.bashrc, ~/.zshrc, etc.):"
    echo ""
    echo "    export PATH=\"\$HOME/.local/bin:\$PATH\""
    echo ""
    echo "Then restart your terminal or run: source ~/.bashrc"
    echo ""
    echo "Or run directly: $INSTALL_DIR/$BINARY_NAME"
else
    echo "🚀 Run with: $BINARY_NAME"
fi

echo ""
echo "📚 GitHub Copilot CLI is also required:"
echo "   npm install -g copilot"
echo ""
