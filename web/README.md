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

🎮 **Multiplayer Mode** - Play against other humans!
- 👥 **Play with Friend** - Share room ID via chat/email
- 🔗 **Join Room** - Enter specific room ID to join
- 🌐 **Serverless P2P** - Direct browser-to-browser connection
- 🏳️ **Resign/Draw** - Full game control

## Multiplayer Technical Details

### How It Works
- **Technology**: WebRTC peer-to-peer connections via [PeerJS](https://peerjs.com/)
- **Signaling**: Free PeerJS cloud server for connection setup
- **Data Transfer**: Direct browser-to-browser after connection established
- **No Backend**: Completely serverless architecture

### Multiplayer Modes

1. **Play with Friend** (`mp-friend`)
   - Creates a unique room with shareable ID
   - Share ID via chat, email, or any messaging platform
   - Friend enters ID using `mp-join` command
   - Instant connection, host plays White

2. **Join Room** (`mp-join`)
   - Enter specific room ID to connect
   - Great for organized games or playing with friends

### Connection Features
- **Turn Validation**: Can't move on opponent's turn
- **Move Sync**: Moves instantly transmitted via WebRTC
- **Board Independence**: Each player chooses blindfold/board visibility
- **Analytics Independence**: Each player controls their own analytics
- **Disconnect Handling**: Graceful error messages on connection loss
- **Resign/Draw**: Standard chess game controls

### Limitations
- Relies on PeerJS cloud service (free tier)
- **Chrome Compatibility**: Chrome may block WebSocket connections due to security policies or extensions. **Recommended browsers: Edge or Firefox**
- NAT traversal may fail in restrictive corporate networks
- No persistent game storage (game lost on disconnect)
- No ELO ratings or match history (yet)

### Troubleshooting Multiplayer

**"Lost connection to server" error (Chrome):**
- **Solution 1**: Use Edge or Firefox instead (fully compatible)
- **Solution 2**: Try Chrome incognito mode (Ctrl+Shift+N)
- **Solution 3**: Disable Chrome extensions (especially adblockers, VPNs, privacy tools)
- **Solution 4**: Check if corporate firewall blocks WebSocket connections

**Connection works in Edge/Firefox but not Chrome:**
- This is a known Chrome issue with PeerJS WebSocket connections
- Chrome has stricter security policies that may interfere
- Extensions like uBlock Origin, Privacy Badger can block WebSockets

## Architecture

- **index.html** - Main game interface
- **style.css** - Styling and responsive design
- **app.js** - Game logic, Stockfish integration, multiplayer manager, UI handling
- **chess.js** (CDN) - Chess move validation library
- **peerjs** (CDN) - WebRTC peer-to-peer connections

## Technology Stack

- Pure vanilla JavaScript (no build step required)
- Chess.js v0.10.3 for chess logic
- PeerJS v1.5.2 for multiplayer WebRTC connections
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
