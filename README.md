# Blindfold Chess - Console Training Application

A C# console application for learning **blindfold chess**. Play against AI without seeing the board - train your visualization skills!

Homepage see https://bertt.github.io/blindfoldchess/

## Features

✨ **Blindfold Mode** - Board hidden by default, visualize in your head  
♟️ **Complete Chess Rules** - Castling, en passant, promotion, check/checkmate/stalemate  
🤖 **AI Opponent** - Powered by Stockfish 17 NNUE chess engine  
🎯 **3 Difficulty Levels** - Beginner (~1000 ELO, Depth 10), Intermediate (~1800 ELO, Depth 12), Advanced (~2400 ELO, Depth 15)  
📊 **Position Analysis** - Real-time material and strategic evaluation  
👀 **Peek Function** - Type 'show' when stuck (but resist!)  
🎨 **Colorblind-Friendly** - Clear colors that work for all vision types  
🔍 **Debug Mode** - View API requests/responses

## Requirements

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

You play **WHITE** (♟), computer plays **BLACK** (♙). Enter moves in standard algebraic notation:

```
Your move (white) > e4
✓ Move played: e4 (Pawn to e4)
📊 Analysis: Material: White 39 - Black 39 (difference: +0)

💻 Computer move: e5 (Pawn to e5)
📜 Moves: 1. e2e4 e7e5

Your move (white) > Nf3
✓ Move played: Nf3 (Knight to f3)

Your move (white) > show
👀 PEEKING - Current board position:
   ┌────────────────────────┐
 8 │ r  n  b  q  k  b  n  r │  ← Black pieces (magenta)
 7 │ p  p  p  p  .  p  p  p │
 6 │ .  .  .  .  .  .  .  . │
 5 │ .  .  .  .  p  .  .  . │
 4 │ .  .  .  .  P  .  .  . │
 3 │ .  .  .  .  .  N  .  . │
 2 │ P  P  P  P  .  P  P  P │  ← White pieces (bright white)
 1 │ R  N  B  Q  K  B  .  R │
   └────────────────────────┘
     a  b  c  d  e  f  g  h

Colors: White pieces (P R N B Q K) in bright white
        Black pieces (p r n b q k) in magenta (colorblind-friendly)
```

Type `help` for all commands.

## Commands

### Moves (Standard Algebraic Notation)
```
e4          Pawn to e4
Nf3         Knight to f3
Bxc4        Bishop captures on c4
exd5        Pawn on e-file captures on d5
O-O         Kingside castling
O-O-O       Queenside castling
e8=Q        Pawn promotion to Queen
Nbd2        Knight from b-file to d2 (when disambiguation needed)
```

Also accepts **coordinate notation** for compatibility:
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
| `debug` | `d` | Show API requests/responses |
| `level` | `l` | Change difficulty |
| `version` | `v` | Version info |
| `update` | - | Check for updates |
| `new` | - | New game |
| `quit` | `q`, `exit` | Exit |

### Difficulty Shortcuts
- `beginner` or `1` - Simple tactical moves (~1000 rating)
- `intermediate` or `2` - Solid play with tactics (~1800 rating, default)
- `advanced` or `3` - Strong strategic play, master level (~2400 rating)

## Stockfish 17 Engine

The AI opponent uses **Stockfish 17 NNUE** - one of the strongest chess engines in the world.

| Difficulty | Depth | Estimated Rating | Description |
|------------|-------|------------------|-------------|
| **Beginner** | 10 | ~1000 | Simple tactical moves, good for learning |
| **Intermediate** | 12 | ~1800 | Solid play with tactics (default) |
| **Advanced** | 15 | ~2400 | Strong strategic play, master level |

**About Stockfish:**
- Open source chess engine, consistently rated 3500+ ELO
- Uses NNUE (Efficiently Updatable Neural Network) for evaluation
- World Computer Chess Champion
- Powers analysis on chess.com, lichess.org and other platforms

## Chess Pieces

The board uses colored letter notation for clarity:

**White pieces** (bright white color): K King, Q Queen, R Rook, B Bishop, N Knight, P Pawn  
**Black pieces** (magenta/purple color): k king, q queen, r rook, b bishop, n knight, p pawn

Colors are optimized for **colorblind accessibility** and work on both light and dark terminal backgrounds.

## Coordinates

The board uses Unicode box-drawing characters with colored pieces:

```
   ┌────────────────────────┐
 8 │ r  n  b  q  k  b  n  r │  ← Black (magenta)
 7 │ p  p  p  p  p  p  p  p │
 6 │ .  .  .  .  .  .  .  . │
 5 │ .  .  .  .  .  .  .  . │
 4 │ .  .  .  .  .  .  .  . │
 3 │ .  .  .  .  .  .  .  . │
 2 │ P  P  P  P  P  P  P  P │  ← White (bright white)
 1 │ R  N  B  Q  K  B  N  R │
   └────────────────────────┘
     a  b  c  d  e  f  g  h
```

**Files:** a-h (left to right)  
**Ranks:** 1-8 (bottom to top)  
**Colors:** Bright white for White pieces, Magenta for Black pieces (colorblind-friendly)

## Troubleshooting

**Invalid move:**
- Use standard algebraic notation: `e4`, `Nf3`, `Bxc4`, `O-O`
- Or coordinate notation: `e2e4`, `g1f3`
- No spaces, check piece symbols (K=King, Q=Queen, R=Rook, B=Bishop, N=Knight)
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
- **Chess.AI** - Stockfish 17 integration via Chess-API.com, position analysis, difficulty levels
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
- **Stockfish 17** - Open source chess engine
- **Chess-API.com** - Free Stockfish API by ChrisC
- C# and love for chess ♟️

---

**Enjoy your blindfold chess training! 🎯♟️**
