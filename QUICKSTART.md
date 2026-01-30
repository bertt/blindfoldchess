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

- `e2e4` - Play a move
- `show` - View the board (peeking!)
- `level` - Change difficulty (beginner/intermediate/advanced)
- `help` - Show all commands
- `quit` - Exit

## First Moves Example

```
> e2e4       (Open with pawn to e4)
> g1f3       (Move knight to f3)
> o-o        (Kingside castling)
> show       (View the board)
```

Enjoy learning blindfold chess! 🎯♟️
