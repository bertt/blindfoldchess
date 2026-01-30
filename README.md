# Console Chess - Blindfold Training Application

A C# console application for learning **blindfold chess**. Play against the computer without seeing the board - train your visualization!

## Features

✨ **Blindfold Mode**: Board is NOT automatically shown - visualize in your head!
♟️ **Complete Chess Rules**: Including castling, en passant, pawn promotion, check/checkmate
🤖 **AI Opponent**: Computer plays black with optional GitHub Copilot SDK integration
📊 **Analysis**: Material evaluation and position analysis after each move
👀 **Peek Function**: Type 'show' to view the board when needed
📜 **Move History**: Track all played moves in algebraic notation

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Windows/Linux/macOS terminal with UTF-8 support
- **GitHub Copilot CLI** - Install with: `npm install -g copilot`
- **Active GitHub Copilot subscription**

## Installation

### 1. Clone or Download
```bash
cd C:\dev\github.com\bertt\consolechess
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build
```bash
dotnet build
```

### 4. Run
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
show/s      - Show the board
help/h      - Show help
moves       - Show move history
analyze/a   - Analyze position
level/l     - Change difficulty level
new         - New game
quit/q      - Exit
```

### Difficulty Levels

The computer has three difficulty levels:

**1. Beginner** - Makes random legal moves
- Perfect for learning the rules
- Easy to beat
- Good for practicing visualization without pressure
- Uses GPT-4o with high temperature (creative/random)

**2. Intermediate** (Default) - Tactical play
- Evaluates material and position
- Prefers captures and center control
- Develops pieces logically
- Challenging for casual players
- Uses GPT-4o with rating ~1500 persona

**3. Advanced** - Strategic depth  
- Uses GPT-4o with rating ~2200 persona
- Plans several moves ahead
- Considers positional factors
- Strong tactical play
- Challenging for intermediate players

**Note**: All levels use GitHub Copilot (GPT-4o) - difficulty is controlled through prompt engineering.

To change level: Type `level` and follow the menu, or directly type `beginner`, `intermediate`, or `advanced`.

See [USER_GUIDE.md](USER_GUIDE.md) for complete documentation.

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

### How It Works

All chess intelligence comes from **GPT-4o** via the Copilot SDK:
- **Position Analysis**: Copilot evaluates positions using FEN notation
- **Move Selection**: Copilot chooses moves based on difficulty level
- **Difficulty via Prompting**: Different system prompts create different playing strengths
  - Beginner: Simple moves, rating ~800
  - Intermediate: Tactical play, rating ~1500
  - Advanced: Strategic depth, rating ~2200

### Troubleshooting

If you see "Copilot analysis error" messages:
1. Verify CLI is installed: `copilot --version`
2. Verify authentication: `copilot auth`
3. Check your Copilot subscription status
4. The app will fallback to basic material evaluation if Copilot is unavailable

## Development

### Build
```bash
dotnet build
```

### Run Tests (TODO)
```bash
dotnet test
```

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
