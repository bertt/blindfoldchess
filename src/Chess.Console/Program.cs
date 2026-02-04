using Chess;
using Chess.AI;

namespace Chess.Console;

class Program
{
    public static readonly string Version = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "0.1.0";
    
    static async Task Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        // Handle command-line arguments
        if (args.Length > 0)
        {
            var arg = args[0].ToLower();
            
            if (arg == "--help" || arg == "-h")
            {
                ShowCommandLineHelp();
                return;
            }
            
            if (arg == "--version" || arg == "-v")
            {
                ShowCommandLineVersion();
                return;
            }
            
            // Unknown argument
            System.Console.WriteLine($"Unknown argument: {args[0]}");
            System.Console.WriteLine("Use --help for usage information");
            return;
        }
        
        // Normal startup
        System.Console.WriteLine("╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║   Blindfold Chess - Train Your Vision      ║");
        System.Console.WriteLine($"║              Version {Version,-22}║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("Goal: Learn blindfold chess! The board is NOT automatically shown.");
        System.Console.WriteLine("Type 'help' for commands, 'show' to see the board.");
        System.Console.WriteLine();

        // No dependencies needed - ready to play!
        System.Console.WriteLine();

        var game = new ChessGame();
        await game.Run();
    }
    
    static void ShowCommandLineHelp()
    {
        System.Console.WriteLine("Blindfold Chess - Console Training Application");
        System.Console.WriteLine($"Version {Version}");
        System.Console.WriteLine();
        System.Console.WriteLine("A C# console application for learning blindfold chess.");
        System.Console.WriteLine("Play against Stockfish without seeing the board - train your visualization!");
        System.Console.WriteLine();
        System.Console.WriteLine("USAGE:");
        System.Console.WriteLine("  blindfoldchess              Start the game");
        System.Console.WriteLine("  blindfoldchess --help       Show this help");
        System.Console.WriteLine("  blindfoldchess --version    Show version information");
        System.Console.WriteLine();
        System.Console.WriteLine("QUICK START:");
        System.Console.WriteLine("  1. Run: blindfoldchess");
        System.Console.WriteLine("  2. Make moves in standard algebraic notation (e.g., e4, Nf3, Bxc4)");
        System.Console.WriteLine("  3. Type 'help' in-game for all commands");
        System.Console.WriteLine();
        System.Console.WriteLine("PROJECT:");
        System.Console.WriteLine("  GitHub: https://github.com/bertt/blindfoldchess");
        System.Console.WriteLine();
    }
    
    static void ShowCommandLineVersion()
    {
        System.Console.WriteLine($"Blindfold Chess v{Version}");
        System.Console.WriteLine($".NET Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        System.Console.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        System.Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
        System.Console.WriteLine();
        System.Console.WriteLine("GitHub: https://github.com/bertt/blindfoldchess");
    }
}

class ChessGame
{
    private ChessBoard _board;
    private ChessApiAnalyzer _analyzer;
    private bool _isRunning = true;
    private bool _showAnalytics = true;

    public ChessGame()
    {
        _board = new ChessBoard();
        _analyzer = new ChessApiAnalyzer();
    }

    public async Task Run()
    {
        try
        {
            System.Console.WriteLine($"New game started! You play WHITE, computer plays BLACK");
            System.Console.WriteLine($"Difficulty: {_analyzer.Difficulty}");
            System.Console.WriteLine();
            
            while (_isRunning && !_board.IsEndGame)
            {
                if (_board.Turn == PieceColor.White)
                {
                    await PlayerTurn();
                }
                else
                {
                    await ComputerTurn();
                }

                // Check game over
                if (_board.IsEndGame)
                {
                    System.Console.WriteLine();
                    if (_board.EndGame?.EndgameType == EndgameType.Checkmate)
                    {
                        System.Console.WriteLine($"═══ CHECKMATE! {_board.EndGame.WonSide} wins! ═══");
                    }
                    else if (_board.EndGame?.EndgameType == EndgameType.Stalemate)
                    {
                        System.Console.WriteLine("═══ STALEMATE! Draw ═══");
                    }
                    else
                    {
                        System.Console.WriteLine($"═══ GAME OVER! {_board.EndGame?.EndgameType} ═══");
                    }
                    ShowBoard();
                    break;
                }
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Thanks for playing! Press any key to exit...");
            System.Console.ReadKey();
        }
        finally
        {
            await _analyzer.DisposeAsync();
        }
    }

    private async Task PlayerTurn()
    {
        if (_board.WhiteKingChecked)
        {
            System.Console.WriteLine("⚠️  You are in CHECK!");
        }

        System.Console.Write("\nYour move (white) > ");
        var input = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(input))
            return;

        // Handle commands (case-insensitive)
        if (await HandleCommand(input.ToLower()))
            return;

        // Try to make move using Gera.Chess
        try
        {
            // Normalize move notation (allow lowercase piece letters)
            var normalizedMove = NormalizeMoveNotation(input);
            _board.Move(normalizedMove);  // Gera.Chess handles SAN, LAN, and more
            
            // Get the last executed move
            if (_board.ExecutedMoves.Count == 0)
            {
                System.Console.WriteLine("❌ Move was not executed");
                return;
            }
            
            var move = _board.ExecutedMoves[_board.ExecutedMoves.Count - 1];
            
            System.Console.WriteLine($"✓ Move played: {move.San}");
            
            if (_showAnalytics)
            {
                // Show thinking indicator while analyzing
                System.Console.Write("💭 Analyzing position...");
                System.Console.Out.Flush();
                
                if (move.CapturedPiece != null)
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine($"  ⚔️  Captured: {move.CapturedPiece.Type} (gained {GetPieceValue(move.CapturedPiece.Type)} points)");
                    System.Console.ResetColor();
                }

                // Check for promotion - it's part of the move's SAN notation
                if (move.San.Contains("="))
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"  👑 Pawn promoted!");
                    System.Console.ResetColor();
                }

                // Check for castling - it's in the SAN notation
                if (move.San.Contains("O-O"))
                {
                    System.Console.ForegroundColor = ConsoleColor.Cyan;
                    System.Console.WriteLine($"  🏰 Castled - King is safer now!");
                    System.Console.ResetColor();
                }

                // Check if black is in check after white's move
                if (_board.BlackKingChecked)
                {
                    System.Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Console.WriteLine($"  ✨ You put BLACK in check!");
                    System.Console.ResetColor();
                }

                // Show analysis
                var analysis = await _analyzer.AnalyzePosition(_board);
                
                // Clear the "thinking" line
                System.Console.Write("\r" + new string(' ', 50) + "\r");
                
                ShowAnalysis(analysis);
                
                // Show additional position info
                ShowPositionInfo();
                
                ShowMoveHistory();
            }
        }
        catch (Exception ex)
        {
            // Provide clearer error message than the raw regex pattern
            string errorMsg = ex.Message;
            
            // If it's the SAN pattern error from Gera.Chess, simplify it
            if (errorMsg.Contains("SAN move string should match pattern"))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Invalid move!");
                System.Console.ResetColor();
                System.Console.WriteLine("   That move is not legal in the current position.");
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"❌ Error: {errorMsg}");
                System.Console.ResetColor();
            }
            
            System.Console.WriteLine();
            System.Console.WriteLine("   Examples of valid moves:");
            System.Console.WriteLine("   • Pawn moves: e4, d5, exd5 (capture)");
            System.Console.WriteLine("   • Piece moves: Nf3, Bc4, Qh5, Rd1");
            System.Console.WriteLine("   • Castling: O-O (kingside), O-O-O (queenside)");
            System.Console.WriteLine("   • Promotion: e8=Q, e8=N");
            System.Console.WriteLine("   • Coordinate: e2e4, g1f3, e7e8q");
            System.Console.WriteLine();
            System.Console.WriteLine("   Type 'show' to see the board");
        }
    }

    private async Task ComputerTurn()
    {
        if (_board.BlackKingChecked)
        {
            System.Console.WriteLine("\n💻 Computer is in check!");
        }

        System.Console.WriteLine("\n💻 Computer is thinking...");
        
        try
        {
            var moveStr = await _analyzer.GetBestMove(_board, PieceColor.Black);
            
            if (moveStr != null)
            {
                _board.Move(moveStr);
                
                if (_board.ExecutedMoves.Count == 0)
                {
                    System.Console.WriteLine("❌ Computer move failed!");
                    _isRunning = false;
                    return;
                }
                
                var move = _board.ExecutedMoves[_board.ExecutedMoves.Count - 1];
                
                System.Console.WriteLine($"💻 Computer move: {move.San}");
                
                if (_showAnalytics)
                {
                    if (move.CapturedPiece != null)
                    {
                        System.Console.WriteLine($"   Captured: {move.CapturedPiece.Type}");
                    }

                    // Show analysis
                    var analysis = await _analyzer.AnalyzePosition(_board);
                    ShowAnalysis(analysis);
                    ShowMoveHistory();
                }
            }
            else
            {
                System.Console.WriteLine("❌ Computer cannot make a move!");
                _isRunning = false;
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            System.Console.ResetColor();
            System.Console.WriteLine("   Chess API is not responding correctly.");
            System.Console.WriteLine("   Please check your internet connection.");
            System.Console.WriteLine();
            System.Console.Write("   Retry? (y/n): ");
            
            var retry = System.Console.ReadLine()?.Trim().ToLower();
            if (retry == "y" || retry == "yes")
            {
                await ComputerTurn(); // Retry
            }
            else
            {
                System.Console.WriteLine("   Game paused. Type 'quit' to exit or make your move to continue.");
            }
        }
    }

    private async Task MakeStockfishMoveForWhite()
    {
        if (_board.Turn != PieceColor.White)
        {
            System.Console.WriteLine("❌ It's not your turn! (Wait for computer to move)");
            return;
        }

        if (_board.WhiteKingChecked)
        {
            System.Console.WriteLine("⚠️  You are in CHECK! Stockfish will try to help...");
        }

        System.Console.WriteLine("\n🎲 YOLO! Asking Stockfish to make a move for you...");
        
        try
        {
            var moveStr = await _analyzer.GetBestMove(_board, PieceColor.White);
            
            if (moveStr != null)
            {
                _board.Move(moveStr);
                
                if (_board.ExecutedMoves.Count == 0)
                {
                    System.Console.WriteLine("❌ Stockfish move failed!");
                    return;
                }
                
                var move = _board.ExecutedMoves[_board.ExecutedMoves.Count - 1];
                
                System.Console.WriteLine($"✓ Stockfish played: {move.San}");
                
                if (_showAnalytics)
                {
                    if (move.CapturedPiece != null)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine($"  ⚔️  Captured: {move.CapturedPiece.Type} (gained {GetPieceValue(move.CapturedPiece.Type)} points)");
                        System.Console.ResetColor();
                    }

                    if (move.San.Contains("="))
                    {
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine($"  👑 Pawn promoted!");
                        System.Console.ResetColor();
                    }

                    if (move.San.Contains("O-O"))
                    {
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        System.Console.WriteLine($"  🏰 Castled - King is safer now!");
                        System.Console.ResetColor();
                    }

                    if (_board.BlackKingChecked)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine($"  ✨ Stockfish put BLACK in check!");
                        System.Console.ResetColor();
                    }

                    var analysis = await _analyzer.AnalyzePosition(_board);
                    ShowAnalysis(analysis);
                    ShowPositionInfo();
                    ShowMoveHistory();
                }
            }
            else
            {
                System.Console.WriteLine("❌ Stockfish couldn't find a valid move!");
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            System.Console.ResetColor();
            System.Console.WriteLine("   Stockfish engine could not make a move. You'll need to play manually.");
        }
    }

    private void TakebackMove()
    {
        // Take back the last full turn (player + computer moves)
        if (_board.ExecutedMoves.Count < 2)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("❌ Cannot take back - not enough moves played yet.");
            System.Console.ResetColor();
            System.Console.WriteLine("   You need at least one full turn (your move + computer's response).");
            return;
        }

        try
        {
            // Get the moves we're undoing for display
            var computerMove = _board.ExecutedMoves[_board.ExecutedMoves.Count - 1];
            var playerMove = _board.ExecutedMoves[_board.ExecutedMoves.Count - 2];

            // Recreate board by replaying all moves except last 2
            var newBoard = new ChessBoard();
            for (int i = 0; i < _board.ExecutedMoves.Count - 2; i++)
            {
                newBoard.Move(_board.ExecutedMoves[i].San);
            }
            
            _board = newBoard;

            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine("↩️  Takeback successful!");
            System.Console.ResetColor();
            System.Console.WriteLine($"   Undid your move: {playerMove.San}");
            System.Console.WriteLine($"   Undid computer's move: {computerMove.San}");
            System.Console.WriteLine();
            
            if (_showAnalytics)
            {
                ShowMoveHistory();
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"❌ Error taking back move: {ex.Message}");
            System.Console.ResetColor();
        }
    }

    private async Task<bool> HandleCommand(string input)
    {
        switch (input)
        {
            case "help":
            case "h":
            case "?":
                ShowHelp();
                return true;

            case "show":
            case "board":
            case "s":
                System.Console.WriteLine("\n👀 PEEKING - Current board position:");
                ShowBoard();
                return true;

            case "moves":
            case "history":
                ShowMoveHistory();
                return true;

            case "analyze":
            case "a":
                var analysis = await _analyzer.AnalyzePosition(_board);
                ShowAnalysis(analysis);
                return true;

            case "debug":
            case "d":
                ShowDebugInfo();
                return true;

            case "level":
            case "difficulty":
            case "l":
                ShowLevelMenu();
                return true;

            case "beginner":
            case "1":
                _analyzer.Difficulty = DifficultyLevel.Beginner;
                System.Console.WriteLine("✓ Difficulty set to: Beginner (random moves)");
                return true;

            case "intermediate":
            case "2":
                _analyzer.Difficulty = DifficultyLevel.Intermediate;
                System.Console.WriteLine("✓ Difficulty set to: Intermediate (tactical play)");
                return true;

            case "advanced":
            case "3":
                _analyzer.Difficulty = DifficultyLevel.Advanced;
                System.Console.WriteLine("✓ Difficulty set to: Advanced (strategic depth)");
                return true;

            case "analytics":
            case "stats":
                _showAnalytics = !_showAnalytics;
                System.Console.ForegroundColor = _showAnalytics ? ConsoleColor.Green : ConsoleColor.Yellow;
                System.Console.WriteLine($"✓ Move analytics {(_showAnalytics ? "ENABLED" : "DISABLED")}");
                System.Console.ResetColor();
                if (_showAnalytics)
                {
                    System.Console.WriteLine("  You will see detailed analysis after each move");
                }
                else
                {
                    System.Console.WriteLine("  Minimal feedback mode - focus on visualization!");
                }
                return true;

            case "version":
            case "v":
                ShowVersion();
                return true;

            case "update":
                await CheckForUpdates();
                return true;

            case "quit":
            case "exit":
            case "q":
                _isRunning = false;
                return true;

            case "new":
                _board = new ChessBoard();
                System.Console.WriteLine("\n✓ New game started!");
                System.Console.WriteLine($"Difficulty: {_analyzer.Difficulty}");
                System.Console.WriteLine($"Analytics: {(_showAnalytics ? "ON" : "OFF")}");
                return true;

            case "yolo":
                await MakeStockfishMoveForWhite();
                return true;

            case "takeback":
            case "undo":
            case "back":
            case "tb":
                TakebackMove();
                return true;

            default:
                return false;
        }
    }

    private void ShowLevelMenu()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║         DIFFICULTY LEVELS                  ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine($"Current: {_analyzer.Difficulty}");
        System.Console.WriteLine();
        System.Console.WriteLine("1. Beginner     - Random legal moves (easy practice)");
        System.Console.WriteLine("2. Intermediate - Tactical play (captures, development)");
        System.Console.WriteLine("3. Advanced     - Strategic depth (3-ply minimax)");
        System.Console.WriteLine();
        System.Console.WriteLine("Type: beginner, intermediate, or advanced (or 1/2/3)");
        System.Console.WriteLine();
    }

    private void ShowBoard()
    {
        System.Console.WriteLine(BoardDisplayHelper.GetAsciiBoard(_board));
    }

    private void ShowAnalysis(AnalysisResult analysis)
    {
        System.Console.WriteLine("\n📊 Analysis:");
        
        if (analysis.Details.ContainsKey("WhiteMaterial") && analysis.Details.ContainsKey("BlackMaterial"))
        {
            int whiteMat = int.Parse(analysis.Details["WhiteMaterial"]);
            int blackMat = int.Parse(analysis.Details["BlackMaterial"]);
            int diff = whiteMat - blackMat;
            
            System.Console.Write($"   Material: White {whiteMat} - Black {blackMat}");
            
            if (diff > 0)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($" (You're ahead by {diff})");
                System.Console.ResetColor();
            }
            else if (diff < 0)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($" (You're behind by {Math.Abs(diff)})");
                System.Console.ResetColor();
            }
            else
            {
                System.Console.WriteLine(" (Equal material)");
            }
        }

        if (!string.IsNullOrEmpty(analysis.Description))
        {
            System.Console.WriteLine($"   Evaluation: {analysis.Description} ({(analysis.Evaluation >= 0 ? "+" : "")}{analysis.Evaluation:F1})");
        }

        if (analysis.Details.ContainsKey("Check"))
        {
            System.Console.WriteLine("   ⚠️  Check!");
        }
    }

    private void ShowPositionInfo()
    {
        var moves = _board.Moves();
        
        System.Console.WriteLine($"   Legal moves: You have {moves.Length}");
        
        // Show move count
        int moveNumber = (_board.ExecutedMoves.Count / 2) + 1;
        System.Console.WriteLine($"   Move #{moveNumber} completed");
    }

    private string NormalizeMoveNotation(string move)
    {
        if (string.IsNullOrEmpty(move))
            return move;

        // Handle lowercase castling: o-o → O-O, o-o-o → O-O-O
        if (move.Equals("o-o", StringComparison.OrdinalIgnoreCase))
            return "O-O";
        if (move.Equals("o-o-o", StringComparison.OrdinalIgnoreCase))
            return "O-O-O";

        // If it starts with a lowercase piece letter (n, b, r, q, k), capitalize it
        // This allows users to type "nc3" instead of "Nc3"
        // BUT: Only if it's NOT a pawn move (e.g., "b4" is a pawn move, not a bishop move)
        // Pawn moves: single letter followed by a digit (e.g., "b4", "e4")
        // Piece moves: piece letter followed by destination or file/rank disambiguator
        if (move.Length >= 2)
        {
            char first = move[0];
            char second = move[1];
            
            // Only capitalize if it's a piece letter AND the second char is NOT a digit
            // This way "b4" stays lowercase (pawn move) but "bc4" becomes "Bc4" (bishop move)
            if ((first == 'n' || first == 'b' || first == 'r' || first == 'q' || first == 'k') 
                && !char.IsDigit(second))
            {
                return char.ToUpper(first) + move.Substring(1);
            }
        }
        
        return move;
    }

    private int GetPieceValue(PieceType type)
    {
        if (type == PieceType.Pawn) return 1;
        if (type == PieceType.Knight) return 3;
        if (type == PieceType.Bishop) return 3;
        if (type == PieceType.Rook) return 5;
        if (type == PieceType.Queen) return 9;
        return 0;
    }

    private void ShowMoveHistory()
    {
        if (_board.ExecutedMoves.Count == 0)
            return;

        System.Console.Write("\n📜 Moves: ");
        for (int i = 0; i < _board.ExecutedMoves.Count; i += 2)
        {
            int moveNum = (i / 2) + 1;
            string white = _board.ExecutedMoves[i].San;
            string black = i + 1 < _board.ExecutedMoves.Count ? _board.ExecutedMoves[i + 1].San : "";
            
            System.Console.Write($"{moveNum}. {white}");
            if (!string.IsNullOrEmpty(black))
            {
                System.Console.Write($" {black}");
            }
            System.Console.Write("  ");
        }
        System.Console.WriteLine();
    }

    private void ShowDebugInfo()
    {
        System.Console.WriteLine("\n🔍 DEBUG - Last Chess API Interaction:");
        System.Console.WriteLine("═══════════════════════════════════════════");
        
        if (string.IsNullOrEmpty(_analyzer.LastRequest))
        {
            System.Console.WriteLine("No Chess API interaction yet.");
            return;
        }

        System.Console.WriteLine("\n📤 REQUEST:");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(_analyzer.LastRequest);
        System.Console.ResetColor();
        
        System.Console.WriteLine("\n📥 RESPONSE:");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine(_analyzer.LastResponse ?? "(no response)");
        System.Console.ResetColor();
        System.Console.WriteLine("═══════════════════════════════════════════");
    }

    private void ShowVersion()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║           VERSION INFORMATION              ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine($"  Blindfold Chess v{Program.Version}");
        System.Console.WriteLine($"  .NET Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        System.Console.WriteLine($"  OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
        System.Console.WriteLine($"  Difficulty: {_analyzer.Difficulty}");
        System.Console.WriteLine($"  Analytics: {(_showAnalytics ? "ON" : "OFF")}");
        System.Console.WriteLine();
        System.Console.WriteLine("  GitHub: https://github.com/bertt/blindfoldchess");
        System.Console.WriteLine("  Update: Type 'update' to check for new versions");
        System.Console.WriteLine();
    }

    private async Task CheckForUpdates()
    {
        System.Console.WriteLine("\n🔍 Checking for updates...");
        
        try
        {
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "blindfoldchess");
            
            var response = await httpClient.GetStringAsync("https://api.github.com/repos/bertt/blindfoldchess/releases/latest");
            
            // Simple JSON parsing for tag_name
            var tagMatch = System.Text.RegularExpressions.Regex.Match(response, "\"tag_name\":\\s*\"([^\"]+)\"");
            if (!tagMatch.Success)
            {
                System.Console.WriteLine("❌ Could not parse version information");
                return;
            }
            
            var latestVersion = tagMatch.Groups[1].Value.TrimStart('v');
            var currentVersion = Program.Version;
            
            System.Console.WriteLine($"\n📌 Current version: v{currentVersion}");
            System.Console.WriteLine($"📌 Latest version:  v{latestVersion}");
            System.Console.WriteLine();
            
            if (latestVersion == currentVersion)
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✅ You are running the latest version!");
                System.Console.ResetColor();
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚡ New version available!");
                System.Console.ResetColor();
                System.Console.WriteLine();
                System.Console.WriteLine("To update, run the installation script:");
                System.Console.WriteLine();
                
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    System.Console.WriteLine("  PowerShell:");
                    System.Console.WriteLine("  irm https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.ps1 | iex");
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    System.Console.WriteLine("  Linux:");
                    System.Console.WriteLine("  curl -fsSL https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.sh | bash");
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    System.Console.WriteLine("  macOS:");
                    System.Console.WriteLine("  curl -fsSL https://raw.githubusercontent.com/bertt/blindfoldchess/main/install.sh | bash");
                }
                
                System.Console.WriteLine();
                System.Console.WriteLine($"Or download from: https://github.com/bertt/blindfoldchess/releases/tag/v{latestVersion}");
            }
        }
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"❌ Error checking for updates: {ex.Message}");
            System.Console.ResetColor();
            System.Console.WriteLine("Please check manually at: https://github.com/bertt/blindfoldchess/releases");
        }
        
        System.Console.WriteLine();
    }

    private void ShowHelp()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║              COMMANDS                      ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("MOVES (Standard Algebraic Notation):");
        System.Console.WriteLine("  e4          - Pawn to e4");
        System.Console.WriteLine("  Nf3 or nf3  - Knight to f3 (case-insensitive)");
        System.Console.WriteLine("  Bxc4        - Bishop captures on c4");
        System.Console.WriteLine("  exd5        - Pawn on e-file captures on d5");
        System.Console.WriteLine("  O-O or o-o  - Kingside castling");
        System.Console.WriteLine("  O-O-O       - Queenside castling (also o-o-o)");
        System.Console.WriteLine("  e8=Q        - Pawn promotion to Queen");
        System.Console.WriteLine("  Nbd2        - Knight from b-file to d2 (disambiguation)");
        System.Console.WriteLine();
        System.Console.WriteLine("  (Also accepts coordinate notation: e2e4, g1f3, e7e8q)");
        System.Console.WriteLine();
        System.Console.WriteLine("COMMANDS:");
        System.Console.WriteLine("  show/s      - 👀 Show the board (peeking!)");
        System.Console.WriteLine("  help/h/?    - Show this help");
        System.Console.WriteLine("  moves       - Show move history");
        System.Console.WriteLine("  takeback/undo/back/tb - ↩️  Undo last full turn");
        System.Console.WriteLine("  analyze/a   - Analyze current position");
        System.Console.WriteLine("  analytics   - 📊 Toggle move analytics ON/OFF");
        System.Console.WriteLine("  debug/d     - 🔍 Show last API request & response");
        System.Console.WriteLine("  level/l     - Change difficulty level");
        System.Console.WriteLine("  yolo        - 🎲 Let Stockfish make a move for you");
        System.Console.WriteLine("  version/v   - Show version information");
        System.Console.WriteLine("  update      - 🔄 Check for updates");
        System.Console.WriteLine("  new         - Start new game");
        System.Console.WriteLine("  quit/q      - Exit");
        System.Console.WriteLine();
        System.Console.WriteLine("PIECES:");
        System.Console.WriteLine("  White: ♚ King  ♛ Queen  ♜ Rook  ♝ Bishop  ♞ Knight  ♟ Pawn");
        System.Console.WriteLine("  Black: ♔ King  ♕ Queen  ♖ Rook  ♗ Bishop  ♘ Knight  ♙ Pawn");
        System.Console.WriteLine();
        System.Console.WriteLine("DIFFICULTY LEVELS:");
        System.Console.WriteLine("  Type 'level' to change: Beginner / Intermediate / Advanced");
        System.Console.WriteLine();
        System.Console.WriteLine("TIPS:");
        System.Console.WriteLine("  - Try to visualize the board in your head");
        System.Console.WriteLine("  - Use 'show' only when you're really stuck");
        System.Console.WriteLine("  - The less you peek, the better you get!");
        System.Console.WriteLine();
    }
}
