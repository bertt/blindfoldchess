// Blindfold Chess - Browser Edition
// Main application logic

class BlindFoldChess {
    constructor() {
        this.chess = new Chess();
        this.difficulty = 'intermediate';
        this.difficultySettings = {
            beginner: { depth: 10, rating: 1000 },
            intermediate: { depth: 12, rating: 1800 },
            advanced: { depth: 15, rating: 2400 }
        };
        this.analyticsEnabled = true;
        this.moveHistory = [];
        this.boardVisible = false;
        this.isComputerThinking = false;
        
        this.init();
    }

    init() {
        const input = document.getElementById('moveInput');
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.handleInput(input.value.trim());
                input.value = '';
            }
        });

        // Auto-scroll terminal
        this.terminal = document.getElementById('terminal');
    }

    handleInput(input) {
        if (!input) return;

        this.addToTerminal(`<span class="terminal-prompt">Your move (${this.chess.turn() === 'w' ? 'white' : 'black'}) &gt;</span> <span class="terminal-move">${input}</span>`);

        const command = input.toLowerCase();

        // Check if it's a command
        if (this.handleCommand(command)) {
            return;
        }

        // Otherwise, treat as a move
        this.makeMove(input);
    }

    handleCommand(command) {
        switch(command) {
            case 'help':
            case 'h':
            case '?':
                this.showHelp();
                return true;

            case 'show':
            case 's':
            case 'board':
                this.toggleBoard();
                return true;

            case 'hide':
                this.toggleBoard(false);
                return true;

            case 'takeback':
            case 'undo':
            case 'back':
            case 'tb':
                this.takeback();
                return true;

            case 'yolo':
                this.yolo();
                return true;

            case 'analytics':
            case 'stats':
                this.toggleAnalytics();
                return true;

            case 'analyze':
            case 'a':
                this.analyze();
                return true;

            case 'new':
                this.newGame();
                return true;

            case 'moves':
            case 'history':
                this.showMoveHistory();
                return true;

            case 'level':
            case 'l':
                this.changeLevel();
                return true;

            case 'beginner':
            case '1':
                this.setDifficulty('beginner');
                return true;

            case 'intermediate':
            case '2':
                this.setDifficulty('intermediate');
                return true;

            case 'advanced':
            case '3':
                this.setDifficulty('advanced');
                return true;

            default:
                return false;
        }
    }

    makeMove(move) {
        if (this.isComputerThinking) {
            this.addError('⏳ Computer is still thinking, please wait...');
            return;
        }

        try {
            const result = this.chess.move(move, { sloppy: true });
            
            if (result) {
                this.addSuccess(`✓ Move played: ${result.san} (${this.describePiece(result.piece)} to ${result.to})`);
                this.moveHistory.push({ player: 'white', move: result.san });
                
                if (this.analyticsEnabled) {
                    this.showMaterialCount();
                }

                if (this.boardVisible) {
                    this.renderBoard();
                }

                // Check for game over
                if (this.chess.game_over()) {
                    this.handleGameOver();
                    return;
                }

                // Computer's turn
                this.makeComputerMove();
            } else {
                this.addError('❌ Invalid move. Use algebraic notation (e.g., e4, Nf3, Bxc4) or type "help" for commands.');
            }
        } catch (error) {
            this.addError(`❌ Invalid move: ${error.message}`);
        }
    }

    async makeComputerMove() {
        this.isComputerThinking = true;
        this.addOutput('<span class="loading">💻 Computer is thinking...</span>');

        try {
            let fen = this.chess.fen();
            const depth = this.difficultySettings[this.difficulty].depth;
            
            // Chess-API.com is stricter about en passant notation
            // Remove en passant square if no actual en passant capture is possible
            const fenParts = fen.split(' ');
            if (fenParts[3] !== '-') {
                // Check if en passant is actually possible
                const epSquare = fenParts[3];
                const moves = this.chess.moves({ verbose: true });
                const hasEpCapture = moves.some(m => m.flags.includes('e') && m.to === epSquare);
                
                if (!hasEpCapture) {
                    fenParts[3] = '-';
                    fen = fenParts.join(' ');
                }
            }
            
            console.log('Requesting move for FEN:', fen);
            
            // Use Chess-API.com (requires POST request)
            const response = await fetch('https://chess-api.com/v1', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    fen: fen,
                    depth: depth
                })
            });
            
            const data = await response.json();
            console.log('Chess-API response:', data);

            if (data.type === 'error') {
                throw new Error(data.error + ': ' + data.text);
            }

            if (data && data.move) {
                // Move is in UCI format (e.g., "e2e4")
                const move = this.chess.move(data.move, { sloppy: true });
                
                if (move) {
                    this.addOutput(`<span class="terminal-computer">💻 Computer move: ${move.san} (${this.describePiece(move.piece)} to ${move.to})</span>`);
                    this.moveHistory.push({ player: 'black', move: move.san });
                    
                    this.addOutput(`📜 Moves: ${this.formatMoveHistory()}`);

                    if (this.boardVisible) {
                        this.renderBoard();
                    }

                    // Check for game over
                    if (this.chess.game_over()) {
                        this.handleGameOver();
                    }
                }
            } else {
                this.addError('❌ Computer could not find a move');
            }
        } catch (error) {
            console.error('Error in makeComputerMove:', error);
            this.addError(`❌ Error getting computer move: ${error.message}`);
        } finally {
            this.isComputerThinking = false;
        }
    }

    async yolo() {
        if (this.isComputerThinking) {
            this.addError('⏳ Computer is still thinking, please wait...');
            return;
        }

        this.addOutput('🎲 YOLO! Asking Stockfish for best move...');

        try {
            const fen = this.chess.fen();
            const response = await fetch('https://chess-api.com/v1', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    fen: fen,
                    depth: 15
                })
            });
            
            const data = await response.json();

            if (data && data.move) {
                const move = this.chess.move(data.move, { sloppy: true });
                
                if (move) {
                    this.addSuccess(`✓ YOLO move: ${move.san} (${this.describePiece(move.piece)} to ${move.to})`);
                    this.moveHistory.push({ player: 'white', move: move.san });
                    
                    if (this.analyticsEnabled) {
                        this.showMaterialCount();
                    }

                    if (this.boardVisible) {
                        this.renderBoard();
                    }

                    if (this.chess.game_over()) {
                        this.handleGameOver();
                        return;
                    }

                    this.makeComputerMove();
                }
            }
        } catch (error) {
            this.addError(`❌ Error getting YOLO move: ${error.message}`);
        }
    }

    takeback() {
        if (this.moveHistory.length < 2) {
            this.addError('❌ No moves to take back');
            return;
        }

        // Undo last two moves (player + computer)
        this.chess.undo();
        this.chess.undo();
        this.moveHistory.pop();
        this.moveHistory.pop();

        this.addSuccess('↩️  Last turn taken back');
        
        if (this.boardVisible) {
            this.renderBoard();
        }
    }

    toggleBoard(show) {
        // If show is undefined, toggle current state
        this.boardVisible = show !== undefined ? show : !this.boardVisible;
        const container = document.getElementById('boardContainer');
        
        if (this.boardVisible) {
            container.style.display = 'block';
            this.renderBoard();
            this.addOutput('👀 PEEKING - Board is now visible');
        } else {
            container.style.display = 'none';
            this.addOutput('🙈 Board hidden - back to blindfold mode');
        }
    }

    renderBoard() {
        const board = this.chess.board();
        const chessboard = document.getElementById('chessboard');
        chessboard.innerHTML = '';

        // Add rank labels and squares
        for (let rank = 7; rank >= 0; rank--) {
            // Rank label
            const rankLabel = document.createElement('div');
            rankLabel.className = 'rank-label';
            rankLabel.textContent = rank + 1;
            chessboard.appendChild(rankLabel);

            // Squares
            for (let file = 0; file < 8; file++) {
                const square = document.createElement('div');
                square.className = `square ${(rank + file) % 2 === 0 ? 'dark' : 'light'}`;
                
                const piece = board[rank][file];
                if (piece) {
                    const pieceDiv = document.createElement('div');
                    pieceDiv.className = `piece ${piece.color === 'w' ? 'white' : 'black'}`;
                    pieceDiv.textContent = this.getPieceSymbol(piece);
                    square.appendChild(pieceDiv);
                }
                
                chessboard.appendChild(square);
            }
        }

        // Empty corner cell
        const corner = document.createElement('div');
        chessboard.appendChild(corner);

        // Add file labels
        const files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        files.forEach(file => {
            const fileLabel = document.createElement('div');
            fileLabel.className = 'file-label';
            fileLabel.textContent = file;
            chessboard.appendChild(fileLabel);
        });
    }

    getPieceSymbol(piece) {
        const symbols = {
            'p': '♟', 'r': '♜', 'n': '♞', 'b': '♝', 'q': '♛', 'k': '♚'
        };
        return symbols[piece.type] || '';
    }

    describePiece(piece) {
        const names = {
            'p': 'Pawn',
            'r': 'Rook',
            'n': 'Knight',
            'b': 'Bishop',
            'q': 'Queen',
            'k': 'King'
        };
        return names[piece] || 'Piece';
    }

    showMaterialCount() {
        const pieces = {
            p: 1, n: 3, b: 3, r: 5, q: 9, k: 0
        };

        let whiteCount = 0;
        let blackCount = 0;

        const board = this.chess.board();
        board.forEach(row => {
            row.forEach(square => {
                if (square) {
                    const value = pieces[square.type];
                    if (square.color === 'w') {
                        whiteCount += value;
                    } else {
                        blackCount += value;
                    }
                }
            });
        });

        const diff = whiteCount - blackCount;
        const diffStr = diff > 0 ? `+${diff}` : diff.toString();
        
        this.addOutput(`📊 Analysis: Material: White ${whiteCount} - Black ${blackCount} (difference: ${diffStr})`);
        
        // Update UI
        document.getElementById('material').textContent = `White ${whiteCount} - Black ${blackCount} (${diffStr})`;
    }

    async analyze() {
        this.addOutput('🔍 Analyzing position...');
        
        try {
            const fen = this.chess.fen();
            const response = await fetch('https://chess-api.com/v1', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    fen: fen,
                    depth: 15,
                    variants: 3
                })
            });
            
            const data = await response.json();

            if (data && data.move) {
                this.addOutput(`💡 Stockfish suggests: ${data.san || data.move}`);
                if (data.eval !== undefined) {
                    this.addOutput(`📊 Evaluation: ${data.eval > 0 ? '+' : ''}${data.eval}`);
                }
                this.addOutput(`🔍 Depth: ${data.depth}`);
                if (data.text) {
                    this.addOutput(`📝 ${data.text}`);
                }
            }
        } catch (error) {
            this.addError(`❌ Error analyzing: ${error.message}`);
        }
        
        this.showMaterialCount();
        const moves = this.chess.moves();
        this.addOutput(`💡 Legal moves: ${moves.length}`);
        if (this.chess.in_check()) {
            this.addOutput('⚠️  King is in check!');
        }
    }

    toggleAnalytics() {
        this.analyticsEnabled = !this.analyticsEnabled;
        this.addSuccess(`📊 Analytics ${this.analyticsEnabled ? 'ON' : 'OFF'}`);
    }

    changeLevel() {
        this.addOutput(`Current difficulty: ${this.difficulty}`);
        this.addOutput(`Available levels: beginner (1), intermediate (2), advanced (3)`);
        this.addOutput(`Type the level name or number to change`);
    }

    setDifficulty(level) {
        this.difficulty = level;
        const settings = this.difficultySettings[level];
        this.addSuccess(`✓ Difficulty set to ${level} (~${settings.rating} rating, depth ${settings.depth})`);
        document.getElementById('difficulty').textContent = level.charAt(0).toUpperCase() + level.slice(1);
    }

    newGame() {
        this.chess.reset();
        this.moveHistory = [];
        this.boardVisible = false;
        document.getElementById('boardContainer').style.display = 'none';
        this.addSuccess('🎮 New game started! You play WHITE (♟), computer plays BLACK (♙)');
        this.addOutput(`Difficulty: ${this.difficulty}`);
    }

    showMoveHistory() {
        if (this.moveHistory.length === 0) {
            this.addOutput('📜 No moves yet');
            return;
        }
        
        this.addOutput('📜 Move history:');
        this.addOutput(this.formatMoveHistory());
    }

    formatMoveHistory() {
        let formatted = '';
        for (let i = 0; i < this.moveHistory.length; i += 2) {
            const moveNum = Math.floor(i / 2) + 1;
            const whiteMove = this.moveHistory[i]?.move || '';
            const blackMove = this.moveHistory[i + 1]?.move || '';
            formatted += `${moveNum}. ${whiteMove} ${blackMove} `;
        }
        return formatted.trim();
    }

    handleGameOver() {
        if (this.chess.in_checkmate()) {
            const winner = this.chess.turn() === 'w' ? 'BLACK' : 'WHITE';
            this.addSuccess(`🎉 Checkmate! ${winner} wins!`);
        } else if (this.chess.in_draw()) {
            this.addOutput('🤝 Game is a draw');
            if (this.chess.in_stalemate()) {
                this.addOutput('(Stalemate)');
            } else if (this.chess.in_threefold_repetition()) {
                this.addOutput('(Threefold repetition)');
            } else if (this.chess.insufficient_material()) {
                this.addOutput('(Insufficient material)');
            }
        }
        this.addOutput(`Type 'new' to start a new game`);
    }

    showHelp() {
        const help = `
COMMANDS:
  show/s/board   - 👀 Toggle board visibility (show/hide)
  hide           - 🙈 Hide the board
  analyze/a      - 🔍 Analyze current position
  analytics      - 📊 Toggle move analytics ON/OFF
  takeback/undo  - ↩️  Undo last full turn
  yolo           - 🎲 Let Stockfish make a move for you
  moves/history  - 📜 Show move history
  level/l        - ℹ️  Show difficulty info
  beginner/1     - Set difficulty to Beginner (~1000)
  intermediate/2 - Set difficulty to Intermediate (~1800)
  advanced/3     - Set difficulty to Advanced (~2400)
  new            - 🎮 Start new game
  help/h/?       - ❓ Show this help

MOVE NOTATION:
  e4             - Pawn to e4
  Nf3            - Knight to f3
  Bxc4           - Bishop captures on c4
  O-O            - Kingside castling
  O-O-O          - Queenside castling
  e8=Q           - Pawn promotion to Queen
        `;
        this.addOutput(help);
    }

    addToTerminal(html) {
        const line = document.createElement('div');
        line.className = 'terminal-line';
        line.innerHTML = html;
        this.terminal.appendChild(line);
        this.terminal.scrollTop = this.terminal.scrollHeight;
    }

    addOutput(text) {
        this.addToTerminal(`<span class="terminal-output">${text}</span>`);
    }

    addSuccess(text) {
        this.addToTerminal(`<span class="terminal-success">${text}</span>`);
    }

    addError(text) {
        this.addToTerminal(`<span class="terminal-error">${text}</span>`);
    }
}

// Execute command from quick buttons
function executeCommand(cmd) {
    const input = document.getElementById('moveInput');
    input.value = cmd;
    input.dispatchEvent(new KeyboardEvent('keypress', { key: 'Enter' }));
}

// Initialize the game when page loads
let game;
window.addEventListener('DOMContentLoaded', () => {
    game = new BlindFoldChess();
});
