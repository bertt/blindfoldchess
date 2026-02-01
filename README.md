# Blindfold Chess - Console Training Application

A C# console application for learning **blindfold chess**. Play against AI without seeing the board - train your visualization skills!

## Features

✨ **Blindfold Mode** - Board hidden by default, visualize in your head  
♟️ **Complete Chess Rules** - Castling, en passant, promotion, check/checkmate/stalemate  
🤖 **AI Opponent** - Powered by GitHub Copilot CLI with multiple AI models  
🎯 **3 Difficulty Levels** - Beginner (~800), Intermediate (~1500), Advanced (~2200)  
📊 **Position Analysis** - Real-time material and strategic evaluation  
👀 **Peek Function** - Type 'show' when stuck (but resist!)  
🔍 **Debug Mode** - View AI prompts/responses for learning

## Requirements

- **GitHub Copilot CLI** (REQUIRED): `npm install -g copilot`
- **Active GitHub Copilot subscription** (REQUIRED): [Subscribe here](https://github.com/settings/copilot)
- **For installation script**: No .NET needed (self-contained executables)
- **For building from source**: [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Installation & Setup

### Quick Install (Recommended) ⚡

**Linux/macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.sh | bash
```

**Windows (PowerShell):**
```powershell
irm https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.ps1 | iex
```

The script will:
- ✅ Auto-detect your OS and architecture
- ✅ Download the latest release
- ✅ Install to `~/.local/bin` (Linux/macOS) or `%LOCALAPPDATA%\blindfoldchess` (Windows)
- ✅ Add to PATH automatically (Windows) or show instructions (Linux/macOS)

Then install GitHub Copilot CLI:
```bash
npm install -g copilot
```
- Requires [Node.js](https://nodejs.org/) (includes npm)
- On first run, browser opens for GitHub authentication - sign in and authorize
- Requires active [GitHub Copilot subscription](https://github.com/settings/copilot)

### Manual Installation

<details>
<summary>Click to expand manual installation options</summary>

#### Option 1: Download Pre-built Release

1. **Download** the latest release for your platform from [Releases](https://github.com/bertt/blindfoldchess/releases):
   - Windows: `blindfoldchess-windows-x64.zip` or `blindfoldchess-windows-arm64.zip`
   - Linux: `blindfoldchess-linux-x64.zip`
   - macOS: `blindfoldchess-macos-x64.zip` or `blindfoldchess-macos-arm64.zip` (M1/M2/M3)

2. **Extract** and run:
   ```bash
   ./blindfoldchess      # Linux/macOS
   blindfoldchess.exe    # Windows
   ```

#### Option 2: Build from Source

```bash
# Install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
git clone https://github.com/bertt/blindfoldchess.git
cd blindfoldchess
dotnet restore
dotnet build
cd src\Chess.Console
dotnet run
```

</details>

## Updating

To update to the latest version, type `update` in the game or re-run the installation script:

**Linux/macOS:**
```bash
curl -fsSL https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.sh | bash
```

**Windows (PowerShell):**
```powershell
irm https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.ps1 | iex
```

## Quick Start

Run the application:
```bash
blindfoldchess              # Start the game
blindfoldchess --help       # Show help
blindfoldchess --version    # Show version info
```

You play **WHITE** (♟), computer plays **BLACK** (♙). Enter moves in algebraic notation:

```
Your move (white) > e2e4
✓ Move played: e2e4 (Pawn to e4)
📊 Analysis: Material: White 39 - Black 39 (difference: +0)

💻 Computer move: e7e5 (Pawn to e5)
📜 Moves: 1. e2e4 e7e5

Your move (white) > show
👀 PEEKING - Current board position:
  a b c d e f g h
8 ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜ 8
7 ♟ ♟ ♟ ♟ . ♟ ♟ ♟ 7
...
```

Type `help` for all commands.

## Commands

### Moves
```
e2e4        Move from e2 to e4
e7e8q       Pawn promotion (q=Queen, r=Rook, b=Bishop, n=Knight)
o-o         Kingside castling
o-o-o       Queenside castling
```

### Game Commands
| Command | Aliases | Description |
|---------|---------|-------------|
| `show` | `s`, `board` | Show board (peeking!) |
| `help` | `h`, `?` | Show help |
| `moves` | `history` | Show move history |
| `analyze` | `a` | Analyze position |
| `analytics` | `stats` | Toggle move analytics ON/OFF (default: ON) |
| `debug` | `d` | Show AI prompt/response |
| `level` | `l` | Change difficulty |
| `model` | `m` | Change AI model |
| `timeout` | `t` | Set timeout (default: 30s, 0=infinite) |
| `yolo` | - | Let Copilot make a move for you |
| `version` | `v` | Version info |
| `update` | - | Check for updates |
| `new` | - | New game |
| `quit` | `q`, `exit` | Exit |

### Difficulty Shortcuts
- `beginner` or `1` - Simple moves (~800 rating)
- `intermediate` or `2` - Tactical play (~1500 rating, default)
- `advanced` or `3` - Strategic depth (~2200 rating)

## AI Models

Change with `model` command:

| Model | Speed | Cost | Strength | Best For |
|-------|-------|------|----------|----------|
| **gpt-4o-mini** | ⚡⚡⚡ | 💰 | 🎯🎯 | Daily practice (default) |
| **gpt-4o** | ⚡⚡ | 💰💰 | 🎯🎯🎯 | Challenging games |
| **claude-sonnet-4.5** | ⚡⚡ | 💰💰 | 🎯🎯🎯 | Positional/creative play |
| **gpt-4.1** | ⚡⚡⚡ | 💰 | 🎯 | Quick practice |

## Chess Pieces

**White** (filled): ♚ King, ♛ Queen, ♜ Rook, ♝ Bishop, ♞ Knight, ♟ Pawn  
**Black** (outline): ♔ King, ♕ Queen, ♖ Rook, ♗ Bishop, ♘ Knight, ♙ Pawn

## Coordinates

```
  a b c d e f g h
8 ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜ 8
7 ♟ ♟ ♟ ♟ ♟ ♟ ♟ ♟ 7
6 . . . . . . . . 6
5 . . . . . . . . 5
4 . . . . . . . . 4
3 . . . . . . . . 3
2 ♙ ♙ ♙ ♙ ♙ ♙ ♙ ♙ 2
1 ♖ ♘ ♗ ♕ ♔ ♗ ♘ ♖ 1
  a b c d e f g h
```

Files: a-h (left to right), Ranks: 1-8 (bottom to top)

## Troubleshooting

**Timeout errors:**
- Default: 30 seconds
- Change: `timeout` command (try 60s or 0 for infinite)
- Switch to faster model: `gpt-4o-mini` or `gpt-4.1`

**Authentication issues:**
- Verify: `copilot --version`
- Check [subscription status](https://github.com/settings/copilot)
- Restart app to re-authenticate

**Invalid move:**
- Use exact coordinates: `e2e4` (not just `e4`)
- No spaces, lowercase letters
- Check if move is legal (doesn't put king in check)

## Tips for Blindfold Chess

1. **Start simple** - Remember just the last 3-5 moves initially
2. **Visualize starting position** - Know the initial board perfectly
3. **Update mentally** - Adjust your mental image after each move
4. **Peek strategically** - Use `show` only to verify when stuck
5. **Track key pieces** - Know where kings and queens are
6. **Practice regularly** - Skills improve with consistent training
7. **Use analytics toggle** - Turn off analytics (`analytics` command) for pure blindfold practice

## Architecture

- **Chess.Core** - Board, pieces, positions, moves, validation, check/checkmate
- **Chess.AI** - Copilot integration, position analysis, difficulty levels
- **Chess.Console** - User interface, blindfold mode, command handling

## Development

```bash
dotnet build              # Build
dotnet test               # Run tests (TODO)
git tag v0.1.0            # Create release tag
git push origin v0.1.0    # Triggers GitHub Actions build
```

GitHub Actions automatically creates releases for all platforms when tags are pushed.

## License

Educational project for learning blindfold chess.

## Credits

- .NET 8.0
- GitHub Copilot SDK 0.1.20
- C# and love for chess ♟️

---

**Enjoy your blindfold chess training! 🎯♟️**
