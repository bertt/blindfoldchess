# Console Chess - Blindfold Training Application

A C# console application for learning **blindfold chess**. Play against the computer without seeing the board - train your visualization!

## Features

✨ **Blindfold Mode**: Board is NOT automatically shown - visualize in your head!
♟️ **Complete Chess Rules**: Including castling, en passant, pawn promotion, check/checkmate/stalemate
🤖 **AI Opponent**: Powered by GitHub Copilot SDK (required) - computer plays black using advanced AI models
🎯 **3 Difficulty Levels**: Beginner (~800), Intermediate (~1500), Advanced (~2200) via prompt engineering
🧠 **Multiple AI Models**: Choose from gpt-4o-mini, gpt-4o, claude-sonnet-4.5, or gpt-4.1
📊 **Position Analysis**: Real-time material evaluation and strategic analysis after each move
👀 **Peek Function**: Type 'show' to view the board when needed (but try not to!)
📜 **Move History**: Track all played moves in algebraic notation
🔍 **Debug Mode**: View AI prompts and responses for learning and troubleshooting

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Windows/Linux/macOS terminal with UTF-8 support
- **GitHub Copilot CLI** (REQUIRED) - Install with: `npm install -g copilot`
- **Active GitHub Copilot subscription** (REQUIRED) - All AI functionality requires Copilot

## Installation

### Option 1: Download Pre-built Release (Recommended)

1. **Download the latest release** for your platform from [Releases](https://github.com/bertt/blindfoldchess/releases)
   - Windows: `blindfoldchess-windows-x64.zip`
   - Linux: `blindfoldchess-linux-x64.zip`
   - macOS Intel: `blindfoldchess-macos-x64.zip`
   - macOS Apple Silicon: `blindfoldchess-macos-arm64.zip`

2. **Extract the archive** to your preferred location

3. **Install GitHub Copilot CLI**:
   ```bash
   npm install -g copilot
   copilot auth
   ```

4. **Run the application**:
   - Windows: Double-click `blindfoldchess.exe` or run in terminal
   - Linux/macOS: `./blindfoldchess` (make executable with `chmod +x blindfoldchess` if needed)

### Option 2: Build from Source

1. **Clone or Download**
   ```bash
   git clone https://github.com/bertt/blindfoldchess.git
   cd blindfoldchess
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build**
   ```bash
   dotnet build
   ```

4. **Run**
   ```bash
   cd src\Chess.Console
   dotnet run
   ```

## Quick Start

1. Start the application
2. You play **WHITE** (♙), computer plays **BLACK** (♟)
3. Enter moves using algebraic notation: `e2e4`
4. Type `help` for all commands
5. Type `show` if you want to see the board (peeking!)

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

## Usage

### Basic Moves
```
e2e4        - Move piece from e2 to e4
d7d5        - Move piece from d7 to d5
```

### Special Moves
```
o-o         - Kingside castling
o-o-o       - Queenside castling
e7e8q       - Pawn promotion to Queen
```

### Commands
```
show/s/board    - Show the board (peeking!)
help/h/?        - Show help
moves/history   - Show move history
analyze/a       - Analyze position
debug/d         - Show last AI prompt & response
level/l         - Change difficulty level
model/m         - Change AI model
version/v       - Show version information
new             - Start new game
quit/q/exit     - Exit
```

### Difficulty Levels

The computer has three difficulty levels:

**1. Beginner** - Simple moves (rating ~800)
- Perfect for learning the rules
- Easy to beat
- Good for practicing visualization without pressure
- Focuses on basic development and center control

**2. Intermediate** (Default) - Tactical play (rating ~1500)
- Evaluates material and position
- Prefers captures and center control
- Develops pieces logically
- Looks for tactical patterns
- Challenging for casual players

**3. Advanced** - Strategic depth (rating ~2200)
- Deep analysis of tactics and strategy
- Plans several moves ahead (3-4 moves)
- Considers king safety, pawn structure, piece activity
- Strong positional and tactical play
- Challenging for intermediate players

**Note**: All levels use GitHub Copilot SDK with configurable AI models - difficulty is controlled through prompt engineering.

**To change level:** 
- Type `level` or `l` to show the menu
- Or directly type: `beginner`, `intermediate`, `advanced`, `1`, `2`, or `3`

See [USER_GUIDE.md](USER_GUIDE.md) for complete documentation.

## Complete Command Reference

### Move Input
| Command | Description | Example |
|---------|-------------|---------|
| `[from][to]` | Basic move | `e2e4`, `g1f3` |
| `[from][to][piece]` | Pawn promotion | `e7e8q` (Queen), `e7e8r` (Rook), `e7e8b` (Bishop), `e7e8n` (Knight) |
| `o-o` | Kingside castling | `o-o` |
| `o-o-o` | Queenside castling | `o-o-o` |

### Game Commands
| Command | Aliases | Description |
|---------|---------|-------------|
| `show` | `s`, `board` | Display the current board position |
| `help` | `h`, `?` | Show help and command list |
| `moves` | `history` | Display move history in algebraic notation |
| `analyze` | `a` | Analyze current position (material, evaluation) |
| `debug` | `d` | Show last AI prompt and response (debugging) |
| `level` | `l`, `difficulty` | Show difficulty level menu |
| `model` | `m` | Show AI model selection menu |
| `version` | `v` | Show version information |
| `new` | - | Start a new game |
| `quit` | `q`, `exit` | Exit the application |

### Difficulty Level Shortcuts
| Command | Description |
|---------|-------------|
| `beginner` or `1` | Set difficulty to Beginner (rating ~800) |
| `intermediate` or `2` | Set difficulty to Intermediate (rating ~1500) |
| `advanced` or `3` | Set difficulty to Advanced (rating ~2200) |

### AI Model Configuration
| Model | Speed | Cost | Strength | Best For |
|-------|-------|------|----------|----------|
| `gpt-4o-mini` | ⚡⚡⚡ | 💰 | 🎯🎯 | Default, daily practice |
| `gpt-4o` | ⚡⚡ | 💰💰 | 🎯🎯🎯 | Challenging games |
| `claude-sonnet-4.5` | ⚡⚡ | 💰💰 | 🎯🎯🎯 | Creative, positional play |
| `gpt-4.1` | ⚡⚡⚡ | 💰 | 🎯 | Quick practice |

## Architecture

The project consists of three modules:

### Chess.Core
- `Board` - Chess board representation and rules
- `Piece` - Chess pieces (King, Queen, Rook, Bishop, Knight, Pawn)
- `Position` - Board positions (a1-h8)
- `Move` - Moves with algebraic notation parsing
- Complete move validation including check/checkmate

### Chess.AI
- `CopilotChessAnalyzer` - AI opponent using GitHub Copilot SDK
- Position analysis (material, threats, strategic evaluation)
- Three difficulty levels via prompt engineering

### Chess.Console
- `Program` - Console interface
- Blindfold mode (no automatic board display)
- Algebraic notation input
- Real-time analysis display

## GitHub Copilot SDK (Required)

This application uses the **GitHub Copilot SDK** which requires the Copilot CLI to be installed locally.

### Setup

1. **Install GitHub Copilot CLI**:
   ```bash
   npm install -g copilot
   ```

2. **Authenticate with GitHub Copilot**:
   ```bash
   copilot auth
   ```
   - Requires an active GitHub Copilot subscription
   - Follow the authentication prompts in your browser

3. **Verify Installation**:
   ```bash
   copilot --version
   ```

4. **Run the Application**:
   ```bash
   cd src\Chess.Console
   dotnet run
   ```

### AI Models

The application supports multiple AI models. Change models with the `model` command.

**Available Models:**

1. **gpt-4o-mini** (Default)
   - ⚡ Fastest | 💰 Cheapest | 🎯 Good chess strength
   - Best for: Quick games, practice, blindfold training

2. **gpt-4o**
   - ⚖️ Balanced | 💰💰 Moderate cost | 🎯🎯 Strong chess
   - Best for: Challenging games, learning tactics

3. **claude-sonnet-4.5**
   - 🧠 Strategic | 💰💰 Moderate cost | 🎯🎯 Creative play
   - Best for: Positional chess, varied openings

4. **gpt-4.1**
   - 🚀 Fast | 💰 Low cost | 🎯 Decent strength
   - Best for: Quick practice games

**To change model:**
- Type `model` or `m` to show the menu
- Select by number (1-4) or name

### How It Works

All chess intelligence comes from AI models via the Copilot SDK:
- **Position Analysis**: AI evaluates positions using FEN notation
- **Move Selection**: AI chooses moves based on difficulty level
- **Difficulty via Prompting**: Different system prompts create different playing strengths
  - Beginner: Simple moves, rating ~800
  - Intermediate: Tactical play, rating ~1500
  - Advanced: Strategic depth, rating ~2200

### Troubleshooting

If you see "Copilot analysis error" messages:
1. Verify CLI is installed: `copilot --version`
2. Verify authentication: `copilot auth`
3. Check your Copilot subscription status
4. Try a different model with the `model` command
5. Check error details with the `debug` command

**Timeout errors:**
- If the AI times out (15-20 seconds), you'll be prompted to retry
- Try switching to a faster model like `gpt-4o-mini` or `gpt-4.1`

## Development

### Build
```bash
dotnet build
```

### Run Tests (TODO)
```bash
dotnet test
```

### Creating a Release

Releases are automatically created when a version tag is pushed:

```bash
# Update version in src/Chess.Console/Chess.Console.csproj
# Then tag and push
git tag v0.1.0
git push origin v0.1.0
```

GitHub Actions will automatically:
- Build for Windows (x64), Linux (x64), macOS (x64 and ARM64)
- Create self-contained single-file executables
- Package them as ZIP files
- Create a GitHub release with all artifacts

**Supported Platforms:**
- `blindfoldchess-windows-x64.zip` - Windows 10/11 (64-bit)
- `blindfoldchess-linux-x64.zip` - Linux (64-bit)
- `blindfoldchess-macos-x64.zip` - macOS Intel (64-bit)
- `blindfoldchess-macos-arm64.zip` - macOS Apple Silicon (M1/M2/M3)

### Project Structure
```
consolechess/
├── src/
│   ├── Chess.Core/          # Chess logic
│   │   ├── Board.cs
│   │   ├── Piece.cs
│   │   ├── Position.cs
│   │   └── Move.cs
│   ├── Chess.AI/            # AI opponent
│   │   ├── CopilotChessAnalyzer.cs
│   │   └── AnalysisResult.cs
│   └── Chess.Console/       # Console interface
│       └── Program.cs
├── ConsoleChess.sln
├── README.md
└── USER_GUIDE.md
```

## Chess Rules Implementation

✅ **Fully implemented:**
- Basic piece movement (Pawn, Knight, Bishop, Rook, Queen, King)
- Check detection
- Checkmate detection
- Stalemate detection
- Castling - kingside and queenside
- En Passant
- Pawn promotion
- Move validation (no illegal moves)

## Tips for Blindfold Chess

1. **Start with short games** - Focus first on 10-15 moves
2. **Visualize the starting board** - Know the initial position by heart
3. **Update mentally** - After each move, adjust the image in your head
4. **Peek strategically** - Use 'show' to verify, not to play
5. **Practice regularly** - The more you practice, the better you get!

## License

This is an educational project for learning blindfold chess.

## Credits

Developed with:
- .NET 8.0
- GitHub Copilot SDK 0.1.20
- C# and love for chess ♟️

## Contributing

This is a personal training project. Suggestions and improvements are welcome!

## Contact

For questions or feedback about blindfold chess training.

---

**Enjoy your blindfold chess training! 🎯♟️**
