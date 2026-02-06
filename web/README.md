# Blindfold Chess - Browser Edition

This is the browser version of Blindfold Chess. Play directly in your browser without any installation required!

## Live Demo

🎮 **[Play Now](https://bertt.github.io/blindfoldchess/web/)**

## Local Development

To test the browser version locally, you have several options:

### Option 1: PowerShell HTTP Server (Windows)
```powershell
# From the project root
.\serve.ps1
# Open http://localhost:8000/web/
```

### Option 2: Node.js (Cross-platform)
```bash
# From the project root
npx http-server -p 8000
# Open http://localhost:8000/web/
```

### Option 3: Python (Cross-platform)
```bash
# From the project root
python -m http.server 8000
# Open http://localhost:8000/web/
```

## Features

✨ **Complete Feature Parity** with console version:
- 👀 Blindfold mode (board hidden by default)
- 🤖 Stockfish 17 integration via Chess-API.com
- 🎯 3 difficulty levels (Beginner, Intermediate, Advanced)
- 📊 Move analytics and material counting
- ↩️ Takeback/undo functionality
- 🎲 YOLO mode (let Stockfish play for you)
- 📜 Move history
- ♟️ Complete chess rules support

## Architecture

- **index.html** - Main game interface
- **style.css** - Styling and responsive design
- **app.js** - Game logic, Stockfish integration, UI handling
- **chess.js** (CDN) - Chess move validation library

## Technology Stack

- Pure vanilla JavaScript (no build step required)
- Chess.js library for chess logic
- Stockfish 17 via Chess-API.com
- Responsive CSS Grid for chessboard
- Terminal-style UI matching console version

## Deployment

Automatically deployed to GitHub Pages via GitHub Actions when pushing to main branch.

See `.github/workflows/pages.yml` for deployment configuration.

## Browser Compatibility

Works on all modern browsers:
- ✅ Chrome/Edge (recommended)
- ✅ Firefox
- ✅ Safari
- ✅ Opera

Requires JavaScript enabled and internet connection for Stockfish API.
