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
        
        // Multiplayer support
        this.isMultiplayer = false;
        this.myColor = 'w'; // 'w' or 'b'
        this.multiplayer = null; // Initialized on first use
        this.waitingForRoomId = false;
        
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

            // Multiplayer commands
            case 'mp-friend':
            case 'play-friend':
                this.getMultiplayer().createGame();
                return true;

            case 'mp-join':
            case 'join':
                this.addOutput('Enter room ID:');
                this.waitingForRoomId = true;
                return true;

            case 'resign':
                if (this.isMultiplayer) {
                    this.getMultiplayer().resign();
                } else {
                    this.addOutput('❌ Resign only available in multiplayer');
                }
                return true;

            case 'draw':
            case 'offer-draw':
                if (this.isMultiplayer) {
                    this.getMultiplayer().offerDraw();
                } else {
                    this.addOutput('❌ Draw offers only available in multiplayer');
                }
                return true;

            default:
                // Check if waiting for room ID
                if (this.waitingForRoomId) {
                    this.waitingForRoomId = false;
                    this.getMultiplayer().joinGame(command);
                    return true;
                }
                return false;
        }
    }

    makeMove(move) {
        if (this.isComputerThinking) {
            this.addError('⏳ Computer is still thinking, please wait...');
            return;
        }

        // In multiplayer, check if it's our turn
        if (this.isMultiplayer) {
            const currentTurn = this.chess.turn();
            if (currentTurn !== this.myColor) {
                this.addError("❌ It's not your turn! Wait for opponent's move.");
                return;
            }
        }

        try {
            // Normalize move notation - make piece letters uppercase
            // Examples: nf3 -> Nf3, Bxc4 (bishop), e4 -> e4, bxc4 (pawn capture stays lowercase)
            let normalizedMove = move;
            
            // Check if first character is a piece letter (not a file)
            const firstChar = move.charAt(0).toLowerCase();
            const secondChar = move.charAt(1);
            
            // Pawn move patterns:
            // - Regular pawn move: e4, a5, h7 (file + rank)
            // - Pawn capture: exd5, bxc4, axb8=Q (file + 'x' + file + rank)
            const isPawnMove = /^[a-h][1-8]($|[=QRBN])/.test(move.toLowerCase());
            const isPawnCapture = /^[a-h]x[a-h][1-8]/.test(move.toLowerCase());
            
            if (['n', 'b', 'r', 'q', 'k'].includes(firstChar) && !isPawnMove && !isPawnCapture) {
                // Capitalize the piece letter for piece moves (not pawn moves/captures)
                normalizedMove = move.charAt(0).toUpperCase() + move.slice(1);
            }
            
            const result = this.chess.move(normalizedMove, { sloppy: true });
            
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

                // In multiplayer, send move to opponent
                if (this.isMultiplayer) {
                    this.getMultiplayer().sendMove(result.san);
                } else {
                    // Computer's turn in single player
                    this.makeComputerMove();
                }
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
                    
                    // Show evaluation score
                    if (data.eval !== undefined) {
                        const evalStr = data.eval > 0 ? `+${data.eval}` : `${data.eval}`;
                        const advantage = data.eval > 0 ? 'White advantage' : data.eval < 0 ? 'Black advantage' : 'Equal';
                        this.addOutput(`📊 Evaluation: ${evalStr} (${advantage})`);
                    }
                    
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
            let fen = this.chess.fen();
            
            // Clean up FEN for chess-api.com (same as makeComputerMove)
            const fenParts = fen.split(' ');
            if (fenParts[3] !== '-') {
                const enPassantSquare = fenParts[3];
                const moves = this.chess.moves({ verbose: true });
                const hasEnPassant = moves.some(move => 
                    move.flags.includes('e') && move.to === enPassantSquare
                );
                if (!hasEnPassant) {
                    fenParts[3] = '-';
                    fen = fenParts.join(' ');
                }
            }
            
            console.log('YOLO FEN:', fen);
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
            console.log('YOLO Response:', data);

            if (data && data.move) {
                const move = this.chess.move(data.move, { sloppy: true });
                
                if (move) {
                    this.addSuccess(`✓ YOLO move: ${move.san} (${this.describePiece(move.piece)} to ${move.to})`);
                    
                    // Show evaluation score
                    if (data.eval !== undefined) {
                        const evalStr = data.eval > 0 ? `+${data.eval}` : `${data.eval}`;
                        const advantage = data.eval > 0 ? 'White advantage' : data.eval < 0 ? 'Black advantage' : 'Equal';
                        this.addOutput(`📊 Evaluation: ${evalStr} (${advantage})`);
                    }
                    
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

        // Add rank labels and squares (rank 8 to 1, so white is at bottom)
        for (let rank = 0; rank < 8; rank++) {
            // Rank label (display 8-rank so rank 0 shows as 8, rank 7 shows as 1)
            const rankLabel = document.createElement('div');
            rankLabel.className = 'rank-label';
            rankLabel.textContent = 8 - rank;
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
        const whiteSymbols = {
            'p': '♙', 'r': '♖', 'n': '♘', 'b': '♗', 'q': '♕', 'k': '♔'
        };
        const blackSymbols = {
            'p': '♟', 'r': '♜', 'n': '♞', 'b': '♝', 'q': '♛', 'k': '♚'
        };
        return piece.color === 'w' ? whiteSymbols[piece.type] : blackSymbols[piece.type] || '';
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

    // Multiplayer methods
    getMultiplayer() {
        if (!this.multiplayer) {
            this.multiplayer = new MultiplayerManager(this);
        }
        return this.multiplayer;
    }

    showMultiplayerMenu() {
        this.addOutput(`
╔════════════════════════════════════════════╗
║          🎮 MULTIPLAYER MODE               ║
╚════════════════════════════════════════════╝

Choose an option:
  mp-friend  - 👥 Play with a friend (share room ID)
  mp-join    - 🔗 Join a room with ID

Type 'new' to start a single player game instead.
        `);
    }

    startMultiplayerGame(myColor) {
        this.isMultiplayer = true;
        this.myColor = myColor;
        this.chess.reset();
        this.moveHistory = [];
        
        const colorName = myColor === 'w' ? 'WHITE' : 'BLACK';
        const symbol = myColor === 'w' ? '♟' : '♙';
        
        this.addOutput(`
╔════════════════════════════════════════════╗
║      MULTIPLAYER GAME STARTED!             ║
╚════════════════════════════════════════════╝

You play: ${colorName} ${symbol}
${myColor === 'w' ? "Your turn! Make your first move." : "Waiting for opponent's move..."}
        `);
        
        this.updatePlayerColor(colorName);
        this.updatePrompt();
        this.updateOnlineCounter();
        
        if (this.boardVisible) {
            this.renderBoard();
        }
    }

    receiveOpponentMove(moveNotation) {
        // Validate it's opponent's turn
        const currentTurn = this.chess.turn();
        if (currentTurn === this.myColor) {
            this.addError('❌ Received move out of turn');
            return;
        }

        // Apply the move
        const move = this.chess.move(moveNotation, { sloppy: true });
        
        if (move) {
            const opponentColor = this.myColor === 'w' ? 'black' : 'white';
            this.addOutput(`<span class="terminal-computer">🌐 Opponent: ${move.san} (${this.describePiece(move.piece)} to ${move.to})</span>`);
            this.moveHistory.push({ player: opponentColor, move: move.san });
            
            this.addOutput(`📜 Moves: ${this.formatMoveHistory()}`);
            
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

            // Now it's our turn
            const myColorName = this.myColor === 'w' ? 'white' : 'black';
            this.addOutput(`<span class="terminal-prompt">Your move (${myColorName}) &gt;</span>`);
        } else {
            this.addError('❌ Invalid move received from opponent');
        }
    }

    updatePlayerColor(colorName) {
        const element = document.getElementById('playerColor');
        if (element) {
            element.textContent = `${colorName} ${colorName === 'WHITE' ? '♟' : '♙'}`;
        }
    }

    updatePrompt() {
        const promptElement = document.getElementById('inputPrompt');
        if (promptElement) {
            const colorName = this.myColor === 'w' ? 'white' : 'black';
            promptElement.textContent = `Your move (${colorName}) >`;
        }
    }

    updateOnlineCounter() {
        const onlineElement = document.getElementById('onlineCount');
        if (onlineElement) {
            if (this.isMultiplayer && this.multiplayer && this.multiplayer.isConnected) {
                onlineElement.textContent = '2'; // This game has 2 players
            } else {
                onlineElement.textContent = '0';
            }
        }
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
        // Disconnect multiplayer if active
        if (this.isMultiplayer && this.multiplayer) {
            this.multiplayer.disconnect();
            this.isMultiplayer = false;
            this.myColor = 'w';
            this.addOutput('🔌 Disconnected from multiplayer');
        }
        
        this.chess.reset();
        this.moveHistory = [];
        this.boardVisible = false;
        document.getElementById('boardContainer').style.display = 'none';
        this.addSuccess('🎮 New game started! You play WHITE (♟), computer plays BLACK (♙)');
        this.addOutput(`Difficulty: ${this.difficulty}`);
        this.updatePlayerColor('WHITE');
        this.updatePrompt();
        this.updateOnlineCounter();
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
  new            - 🎮 Start new game (vs Computer)
  help/h/?       - ❓ Show this help

MULTIPLAYER:
  mp-friend      - 👥 Play with a friend (share room ID)
  mp-join        - 🔗 Join a room with ID
  resign         - 🏳️  Resign (multiplayer only)
  offer-draw     - 🤝 Offer draw (multiplayer only)

MOVE NOTATION:
  e4             - Pawn to e4
  Nf3 or nf3     - Knight to f3 (case insensitive)
  Bxc4 or bxc4   - Bishop captures on c4
  O-O or o-o     - Kingside castling
  O-O-O or o-o-o - Queenside castling
  e8=Q or e8=q   - Pawn promotion to Queen
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

// Multiplayer Manager using PeerJS
class MultiplayerManager {
    constructor(game) {
        this.game = game;
        this.peer = null;
        this.connection = null;
        this.mode = null; // 'host', 'join', 'random'
        this.myColor = null; // 'w' or 'b'
        this.isConnected = false;
        this.roomId = null;
    }

    // Create new game as host (Play Friend mode)
    createGame() {
        this.mode = 'host';
        this.myColor = 'w'; // Host plays white
        
        this.peer = new Peer();
        
        this.peer.on('open', (id) => {
            this.roomId = id;
            this.game.addOutput(`🎮 Room created! Share this ID: <strong>${id}</strong>`);
            this.game.addOutput(`⏳ Waiting for opponent to join...`);
            this.showCopyButton(id);
        });

        this.peer.on('connection', (conn) => {
            this.connection = conn;
            this.setupConnectionListeners();
            this.game.addSuccess('✅ Opponent connected! Game starting...');
            this.isConnected = true;
            this.game.startMultiplayerGame('w');
        });

        this.peer.on('error', (err) => {
            this.game.addError(`❌ Peer error: ${err.type}`);
            console.error('Peer error:', err);
        });
    }

    // Join existing game with room ID
    joinGame(roomId) {
        this.mode = 'join';
        this.myColor = 'b'; // Joiner plays black
        this.roomId = roomId;

        this.peer = new Peer();

        this.peer.on('open', () => {
            this.game.addOutput(`🔗 Connecting to room: ${roomId}...`);
            this.connection = this.peer.connect(roomId);
            this.setupConnectionListeners();
        });

        this.peer.on('error', (err) => {
            console.error('Peer error:', err);
            if (err.type === 'peer-unavailable') {
                this.game.addError(`❌ Room not found. Check the ID and try again.`);
            } else {
                this.game.addError(`❌ Connection error: ${err.type}`);
            }
        });
    }



    setupConnectionListeners() {
        console.log('Setting up connection listeners...');

        this.connection.on('open', () => {
            console.log('Connection opened!');
            this.isConnected = true;
            this.game.addSuccess('✅ Connected! Game starting...');
            this.game.startMultiplayerGame(this.myColor);
        });

        this.connection.on('data', (data) => {
            console.log('Received data:', data);
            this.handleIncomingData(data);
        });

        this.connection.on('close', () => {
            console.log('Connection closed');
            this.game.addError('❌ Opponent disconnected');
            this.isConnected = false;
            this.game.updateOnlineCounter();
        });

        this.connection.on('error', (err) => {
            console.error('Connection error:', err);
            this.game.addError(`❌ Connection error: ${err.message}`);
        });
        
        console.log('Connection listeners setup complete');
    }

    handleIncomingData(data) {
        if (data.type === 'move') {
            this.game.receiveOpponentMove(data.move);
        } else if (data.type === 'resign') {
            this.game.addSuccess('🏳️ Opponent resigned! You win!');
        } else if (data.type === 'draw-offer') {
            this.game.addOutput('🤝 Opponent offers a draw. Type "accept-draw" to accept.');
        }
    }

    sendMove(move) {
        if (this.connection && this.isConnected) {
            this.connection.send({ type: 'move', move: move });
        }
    }

    resign() {
        if (this.connection && this.isConnected) {
            this.connection.send({ type: 'resign' });
            this.game.addOutput('🏳️ You resigned');
            this.disconnect();
        }
    }

    offerDraw() {
        if (this.connection && this.isConnected) {
            this.connection.send({ type: 'draw-offer' });
            this.game.addOutput('🤝 Draw offer sent');
        }
    }

    disconnect() {
        if (this.connection) {
            this.connection.close();
        }
        if (this.peer) {
            this.peer.destroy();
        }
        this.isConnected = false;
        this.connection = null;
        this.peer = null;
    }

    showCopyButton(roomId) {
        const copyBtn = `<button onclick="navigator.clipboard.writeText('${roomId}').then(() => game.addSuccess('✓ Room ID copied!'))">📋 Copy ID</button>`;
        this.game.addToTerminal(copyBtn);
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
