# Blindfold Chess - User Guide

## Introduction

Welcome to **Blindfold Chess**, a console application specially designed to help you learn **blindfold chess**! 

The board is NOT automatically shown - you must visualize the position in your head. This is a powerful training method used by chess grandmasters.

## How to Play

### Starting
```
cd C:\dev\github.com\bertt\consolechess\src\Chess.Console
dotnet run
```

### Blindfold Mode

After starting, you see **NO chess board**. You only see:
- The moves that have been played
- Position analysis (material, evaluation)
- Messages (check, captured pieces, etc.)

You must **visualize the board in your head**!

## Algebraic Notation

All moves are entered in **algebraic notation**:

### Basic Moves
```
e2e4    - Move piece from e2 to e4
d7d5    - Move piece from d7 to d5
g1f3    - Move piece from g1 to f3
```

### Coordinates
- **Files**: a-h (from left to right)
- **Ranks**: 1-8 (from bottom to top)
- White starts on ranks 1-2, Black on ranks 7-8

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

### Special Moves

#### Castling
```
o-o     - Kingside castling - King to g-file
o-o-o   - Queenside castling - King to c-file
```

#### Pawn Promotion
```
e7e8q   - Pawn to e8 and promotes to Queen
e7e8r   - Pawn to e8 and promotes to Rook
e7e8b   - Pawn to e8 and promotes to Bishop
e7e8n   - Pawn to e8 and promotes to Knight
```

#### En Passant
En passant is automatically recognized. Just enter the diagonal pawn move:
```
e5d6    - When black just played d7-d5
```

## Commands

### Game Commands
- **show** or **s** - 👀 Show the board (PEEKING! Use sparingly)
- **help** or **h** or **?** - Show help information
- **moves** or **history** - Show all played moves
- **analyze** or **a** - Analyze the current position
- **level** or **l** - Change difficulty level
- **new** - Start a new game
- **quit** or **q** - Exit

### Difficulty Levels

You can adjust the computer's strength:

#### 1. Beginner
- GPT-4o with beginner persona (rating ~800)
- Makes simple, straightforward moves
- Perfect for learning rules and practicing visualization
- Very easy to beat

#### 2. Intermediate (Default)
- GPT-4o with intermediate persona (rating ~1500)
- Tactical evaluation of positions
- Captures valuable pieces
- Controls the center
- Develops pieces logically
- Good challenge for casual players

#### 3. Advanced
- GPT-4o with master persona (rating ~2200)
- Deep analysis: tactics, strategy, king safety
- Strong positional play
- Piece coordination and pawn structure
- Challenging for intermediate players

**Powered by**: GitHub Copilot (GPT-4o via Azure AI)

**To change level:**
- Type `level` to see the menu
- Or directly type: `beginner`, `intermediate`, or `advanced`
- Or use shortcuts: `1`, `2`, or `3`

## Chess Pieces

### White (you play this color)
- ♔ - King
- ♕ - Queen
- ♖ - Rook
- ♗ - Bishop
- ♘ - Knight
- ♙ - Pawn

### Black (computer plays this color)
- ♚ - King
- ♛ - Queen
- ♜ - Rook
- ♝ - Bishop
- ♞ - Knight
- ♟ - Pawn

## Example Game Flow

```
> e2e4
✓ Move played: e2e4 (Pawn to e4)

📊 Analysis:
   Material: White 39 - Black 39 (difference: +0)
   Evaluation: Equal (+0.3)

💻 Computer is thinking...
💻 Computer move: e7e5 (Pawn to e5)

📊 Analysis:
   Material: White 39 - Black 39 (difference: +0)
   Evaluation: Equal (+0.0)

📜 Moves: 1. e2e4 e7e5

Your move (white) > level
╔════════════════════════════════════════════╗
║         DIFFICULTY LEVELS                  ║
╚════════════════════════════════════════════╝

Current: Intermediate

1. Beginner     - Random legal moves (easy practice)
2. Intermediate - Tactical play (captures, development)
3. Advanced     - Strategic depth (3-ply minimax)

Type: beginner, intermediate, or advanced (or 1/2/3)

Your move (white) > advanced
✓ Difficulty set to: Advanced (strategic depth)

Your move (white) > g1f3
✓ Move played: g1f3 (Knight to f3)
```

## Analysis Information

After each move you see:

### Material
Total value of pieces:
- Pawn = 1 point
- Knight/Bishop = 3 points
- Rook = 5 points
- Queen = 9 points
- King = priceless

### Evaluation
- **Positive number**: White is better
- **Negative number**: Black is better
- **0**: Equal position

Examples:
- `+0.3` - Slight advantage White
- `-2.5` - Slight advantage Black
- `+5.0` - Large advantage White

## Tips for Blindfold Chess

1. **Start Simple**: Begin by remembering just the last 3-5 moves
2. **Visualize Starting Board**: Make sure you see the initial board perfectly
3. **Update Mentally**: After each move, update the image in your head
4. **Peek Strategically**: Use 'show' only to verify when in doubt
5. **Concentration**: Mental chess requires focus - no distractions!
6. **Remember Landmarks**: Keep track of important pieces (where is my king, their queen, etc.)
7. **Repetition**: The more you practice, the better you become

## Common Mistakes

### "Invalid move notation"
- Make sure you use exact coordinates: `e2e4` (not just `e4`)
- Use lowercase letters
- No spaces

### "Invalid move"
The move is not according to chess rules. Possible reasons:
- Piece cannot move that way
- Path is blocked
- Move puts your own king in check
- Castling not possible (pieces already moved or king is in check)

### Castling doesn't work
For castling:
- King and relevant rook must not have moved yet
- No pieces between king and rook
- King must not be in check
- King must not move through check
- King must not end in check

## GitHub Copilot SDK (Required)

The application uses the **GitHub Models API** for all chess analysis.

### Configuration
Set environment variable with your GitHub token:
```
GITHUB_TOKEN=your_github_token_here
```

Get your token from: https://github.com/settings/tokens

### Without Copilot SDK
The app requires the GitHub token to function. All chess intelligence comes from GPT-4o via GitHub Models.

## Have Fun!

Blindfold chess is a challenging but very rewarding skill. It improves:
- Your chess visualization
- Your concentration
- Your memory
- Your tactical insight

Good luck with your training! 🎯♟️
