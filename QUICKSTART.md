# Console Chess - Quick Start

## Prerequisites

Install GitHub Copilot CLI:
```bash
npm install -g copilot
copilot auth
```

## Installation

```bash
cd C:\dev\github.com\bertt\consolechess
dotnet restore
dotnet build
```

## Playing

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
