# Console Chess - Quick Start

## Prerequisites

**Option 1: Download Release (Easiest - No .NET Required)**
- Download from [Releases](https://github.com/bertt/blindfoldchess/releases)
- Self-contained executable - **no .NET installation needed**
- Install GitHub Copilot CLI:
  ```bash
  npm install -g copilot
  ```
  On first run, Copilot authenticates with your GitHub account automatically.
  Requires [GitHub Copilot subscription](https://github.com/settings/copilot).

**Option 2: Build from Source (Requires .NET 8.0 SDK)**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - **required to compile the code**
- GitHub Copilot CLI:
  ```bash
  npm install -g copilot
  ```
  Requires [GitHub Copilot subscription](https://github.com/settings/copilot).

## Installation

**Using Pre-built Release (No .NET Required):**
```bash
# Download and extract the release for your platform
# Windows x64: blindfoldchess-windows-x64.zip
# Windows ARM64: blindfoldchess-windows-arm64.zip
# Linux: blindfoldchess-linux-x64.zip
# macOS Intel: blindfoldchess-macos-x64.zip
# macOS Apple Silicon: blindfoldchess-macos-arm64.zip

# Run the executable - no installation needed!
./blindfoldchess    # Linux/macOS
blindfoldchess.exe  # Windows
```

**Building from Source (Requires .NET 8.0 SDK):**
```bash
# Install .NET 8.0 SDK first from https://dotnet.microsoft.com/download/dotnet/8.0
git clone https://github.com/bertt/blindfoldchess.git
cd blindfoldchess
dotnet restore
dotnet build
```

## Running

**Pre-built Release:**
```bash
./blindfoldchess      # Linux/macOS
blindfoldchess.exe    # Windows
```

**From Source:**
```bash
cd src\Chess.Console
dotnet run
```

## Quick Commands

**Moves:**
- `e2e4` - Play a move
- `o-o` - Kingside castling
- `o-o-o` - Queenside castling
- `e7e8q` - Pawn promotion to Queen

**Game Commands:**
- `show` / `s` - View the board (peeking!)
- `level` / `l` - Change difficulty (beginner/intermediate/advanced or 1/2/3)
- `model` / `m` - Change AI model (gpt-4o-mini/gpt-4o/claude-sonnet-4.5/gpt-4.1)
- `moves` - Show move history
- `analyze` / `a` - Analyze position
- `debug` / `d` - Show AI prompt/response
- `version` / `v` - Show version information
- `help` / `h` - Show all commands
- `new` - New game
- `quit` / `q` - Exit

## First Moves Example

```
> e2e4       (Open with pawn to e4)
> g1f3       (Move knight to f3)
> o-o        (Kingside castling)
> show       (View the board)
```

Enjoy learning blindfold chess! 🎯♟️
