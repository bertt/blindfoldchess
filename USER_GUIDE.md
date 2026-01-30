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
8 ♖ ♘ ♗ ♕ ♔ ♗ ♘ ♖ 8
7 ♟ ♟ ♟ ♟ ♟ ♟ ♟ ♟ 7
6 . . . . . . . . 6
5 . . . . . . . . 5
4 . . . . . . . . 4
3 . . . . . . . . 3
2 ♙ ♙ ♙ ♙ ♙ ♙ ♙ ♙ 2
1 ♜ ♞ ♝ ♛ ♚ ♝ ♞ ♜ 1
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
- **show** / **s** / **board** - 👀 Show the board (PEEKING! Use sparingly)
- **help** / **h** / **?** - Show help information
- **moves** / **history** - Show all played moves
- **analyze** / **a** - Analyze the current position
- **debug** / **d** - 🔍 Show last AI prompt & response (for debugging)
- **level** / **l** / **difficulty** - Change difficulty level
- **model** / **m** - 🤖 Change AI model
- **timeout** / **t** - ⏱️ Set Copilot timeout (default: 30 seconds, 0 = infinite)
- **version** / **v** - Show version information
- **new** - Start a new game
- **quit** / **q** / **exit** - Exit

### Difficulty Levels

You can adjust the computer's strength:

#### 1. Beginner
- AI with beginner persona (rating ~800)
- Makes simple, straightforward moves
- Focuses on basic development and center control
- Perfect for learning rules and practicing visualization
- Very easy to beat

#### 2. Intermediate (Default)
- AI with intermediate persona (rating ~1500)
- Tactical evaluation of positions
- Looks for captures, threats, and tactical patterns
- Controls the center and develops pieces logically
- Avoids hanging pieces
- Good challenge for casual players

#### 3. Advanced
- AI with master persona (rating ~2200)
- Deep analysis: tactics, strategy, king safety
- Calculates 3-4 moves ahead
- Strong positional play and pawn structure evaluation
- Piece coordination and endgame considerations
- Challenging for intermediate players

**Powered by**: GitHub Copilot SDK with configurable AI models

**To change level:**
- Type `level`, `l`, or `difficulty` to see the menu
- Or directly type: `beginner`, `intermediate`, or `advanced`
- Or use shortcuts: `1`, `2`, or `3`

## Chess Pieces

### White (you play this color)
- ♚ - King
- ♛ - Queen
- ♜ - Rook
- ♝ - Bishop
- ♞ - Knight
- ♙ - Pawn

### Black (computer plays this color)
- ♔ - King
- ♕ - Queen
- ♖ - Rook
- ♗ - Bishop
- ♘ - Knight
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

## GitHub Copilot CLI (Required)

The application uses the **[GitHub Copilot CLI](https://www.npmjs.com/package/copilot)** for all chess intelligence.

### What is it?
The GitHub Copilot CLI is a command-line interface that provides access to AI models. This application uses it for:
- Chess move generation
- Position analysis
- Strategic evaluation

### Installation & Setup

1. **Install the CLI**:
   ```bash
   npm install -g copilot
   ```
   
   (Requires Node.js/npm - download from [https://nodejs.org/](https://nodejs.org/))

2. **Verify installation**:
   ```bash
   copilot --version
   ```

### Authentication

**Automatic (Recommended):**
- On first run, a browser window opens automatically
- Sign in to GitHub and authorize the Copilot CLI
- Authentication is saved in `~/.copilot/` and persists

**Requirements:**
- Active [GitHub Copilot subscription](https://github.com/settings/copilot)
- Internet connection (required for all AI functionality)

**Learn more:** [GitHub Copilot CLI Documentation](https://docs.github.com/en/copilot/using-github-copilot/using-github-copilot-in-the-command-line)

### AI Models

You can change the AI model during gameplay with the `model` command.

**Available Models:**

1. **gpt-4o-mini** (Default)
   - ⚡ Fastest response time
   - 💰 Lowest cost
   - 🎯 Good chess strength
   - **Best for:** Quick games, practice, daily blindfold training

2. **gpt-4o**
   - ⚖️ Balanced performance
   - 💰💰 Moderate cost
   - 🎯🎯 Strong chess play
   - **Best for:** Challenging games, learning tactics

3. **claude-sonnet-4.5**
   - 🧠 Strategic thinking
   - 💰💰 Moderate cost
   - 🎯🎯 Creative and varied play
   - **Best for:** Positional chess, exploring different openings

4. **gpt-4.1**
   - 🚀 Fast response
   - 💰 Low cost
   - 🎯 Decent strength
   - **Best for:** Quick practice sessions

**Performance Comparison:**
- **Speed:** gpt-4.1 > gpt-4o-mini > gpt-4o ≈ claude-sonnet-4.5
- **Cost:** gpt-4.1 < gpt-4o-mini < gpt-4o ≈ claude-sonnet-4.5
- **Chess Strength:** gpt-4o > claude-sonnet-4.5 > gpt-4o-mini > gpt-4.1

**To change model:**
- Type `model` or `m` during gameplay
- Select by number (1-4) or enter the model name
- The current session will switch to the new model

### Debug Command

Use the `debug` or `d` command to see:
- The exact prompt sent to the AI
- The AI's raw response
- Useful for troubleshooting move selection issues

### Troubleshooting

**Timeout Errors:**
If the AI times out (doesn't respond within the set timeout):
- Default timeout is 30 seconds
- Change timeout with the `timeout` command
- You can set it to 60, 120 seconds or more
- Set to 0 for infinite timeout (no timeout - waits indefinitely)
- You'll be prompted to retry when a timeout occurs
- Consider switching to a faster model (`gpt-4o-mini` or `gpt-4.1`)
- Check your internet connection
- For slow/unstable connections, use a longer timeout or infinite timeout

**Authentication Issues:**
If Copilot is not working:
1. Verify installation: `copilot --version`
2. Check your [GitHub Copilot subscription status](https://github.com/settings/copilot)
3. Ensure you're signed in to GitHub
4. Check your internet connection (required for Copilot CLI)
5. If authentication fails, restart the application to trigger the authentication flow again

**Model Switch Failures:**
- The application will keep using the current model
- Check the debug output for error details
- Ensure your Copilot subscription is active

## Have Fun!

Blindfold chess is a challenging but very rewarding skill. It improves:
- Your chess visualization
- Your concentration
- Your memory
- Your tactical insight

Good luck with your training! 🎯♟️
