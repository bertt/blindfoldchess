using Chess.Core;
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
        System.Console.WriteLine($"║              Version {Version,-22} ║");
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
        System.Console.WriteLine("Play against AI without seeing the board - train your visualization!");
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
    private Board _board;
    private ChessApiAnalyzer _analyzer;
    private bool _isRunning = true;
    private bool _showAnalytics = true;

    public ChessGame()
    {
        _board = new Board();
        _analyzer = new ChessApiAnalyzer();
    }

    public async Task Run()
    {
        try
        {
            System.Console.WriteLine($"New game started! You play WHITE (♟), computer plays BLACK (♙)");
            System.Console.WriteLine($"Difficulty: {_analyzer.Difficulty}");
            System.Console.WriteLine();

            while (_isRunning)
            {
                if (_board.CurrentTurn == PieceColor.White)
                {
                    await PlayerTurn();
                }
                else
                {
                    await ComputerTurn();
                }

                // Check game over
                if (_board.IsCheckmate(_board.CurrentTurn))
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine($"═══ CHECKMATE! {(_board.CurrentTurn == PieceColor.White ? "Black" : "White")} wins! ═══");
                    ShowBoard();
                    break;
                }

                if (_board.IsStalemate(_board.CurrentTurn))
                {
                    System.Console.WriteLine();
                    System.Console.WriteLine("═══ STALEMATE! Draw ═══");
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
        if (_board.IsInCheck(PieceColor.White))
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

        // Try to parse and make move
        try
        {
            Move? move = null;
            
            // First try to parse as SAN
            move = Move.ParseSAN(input, _board);
            
            // If SAN parsing failed, try long algebraic notation
            if (move == null)
            {
                try
                {
                    move = new Move(input);
                    
                    // Adjust castling for white
                    if (move.IsCastling)
                    {
                        move.From = new Position(0, 4);
                        move.To = new Position(0, move.To.Col == 6 ? 6 : 2);
                    }
                }
                catch
                {
                    System.Console.WriteLine($"❌ Invalid move: {input}");
                    System.Console.WriteLine("Use algebraic notation (e.g., e4, Nf3, Bxc4) or coordinate notation (e.g., e2e4)");
                    return;
                }
            }
            
            if (move == null)
            {
                System.Console.WriteLine($"❌ Invalid move: {input}");
                return;
            }

            // Get SAN before making the move (need board state)
            string sanMove = _board.GetMoveSAN(move);
            
            if (_board.MakeMove(move))
            {
                var piece = _board.GetPiece(move.To);
                string pieceName = piece?.GetName() ?? "?";
                
                System.Console.WriteLine($"✓ Move played: {sanMove} ({pieceName} to {move.To.ToAlgebraic()})");
                
                if (_showAnalytics)
                {
                    if (move.CapturedPiece != null)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine($"  ⚔️  Captured: {move.CapturedPiece.GetName()} (gained {GetPieceValue(move.CapturedPiece.Type)} points)");
                        System.Console.ResetColor();
                    }

                    if (move.PromotionPiece != null)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine($"  👑 Promoted to: {move.PromotionPiece.Value}");
                        System.Console.ResetColor();
                    }

                    if (move.IsCastling)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        System.Console.WriteLine($"  🏰 Castled - King is safer now!");
                        System.Console.ResetColor();
                    }

                    // Check if black is in check after white's move
                    if (_board.IsInCheck(PieceColor.Black))
                    {
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine($"  ✨ You put BLACK in check!");
                        System.Console.ResetColor();
                    }

                    // Show analysis
                    var analysis = await _analyzer.AnalyzePosition(_board);
                    ShowAnalysis(analysis);
                    
                    // Show additional position info
                    ShowPositionInfo();
                    
                    ShowMoveHistory();
                }
            }
            else
            {
                System.Console.WriteLine("❌ Invalid move! Try again.");
                System.Console.WriteLine("   Examples: e4, Nf3, Bxc4, O-O, e8=Q");
                System.Console.WriteLine("   Or use coordinate notation: e2e4, g1f3");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            System.Console.WriteLine("   Type 'help' for instructions");
        }
    }

    private async Task ComputerTurn()
    {
        if (_board.IsInCheck(PieceColor.Black))
        {
            System.Console.WriteLine("\n💻 Computer is in check!");
        }

        System.Console.WriteLine("\n💻 Computer is thinking...");
        
        try
        {
            var move = await _analyzer.GetBestMove(_board, PieceColor.Black);
            
            if (move != null)
            {
                string sanMove = _board.GetMoveSAN(move);
                
                _board.MakeMove(move);
                var piece = _board.GetPiece(move.To);
                string pieceName = piece?.GetName() ?? "?";
                
                System.Console.WriteLine($"💻 Computer move: {sanMove} ({pieceName} to {move.To.ToAlgebraic()})");
                
                if (_showAnalytics)
                {
                    if (move.CapturedPiece != null)
                    {
                        System.Console.WriteLine($"   Captured: {move.CapturedPiece.GetName()}");
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
        if (_board.CurrentTurn != PieceColor.White)
        {
            System.Console.WriteLine("❌ It's not your turn! (Wait for computer to move)");
            return;
        }

        if (_board.IsInCheck(PieceColor.White))
        {
            System.Console.WriteLine("⚠️  You are in CHECK! Stockfish will try to help...");
        }

        System.Console.WriteLine("\n🎲 YOLO! Asking Stockfish to make a move for you...");
        
        try
        {
            var move = await _analyzer.GetBestMove(_board, PieceColor.White);
            
            if (move != null)
            {
                string sanMove = _board.GetMoveSAN(move);
                
                _board.MakeMove(move);
                var piece = _board.GetPiece(move.To);
                string pieceName = piece?.GetName() ?? "?";
                
                System.Console.WriteLine($"✓ Stockfish played: {sanMove} ({pieceName} to {move.To.ToAlgebraic()})");
                
                if (_showAnalytics)
                {
                    if (move.CapturedPiece != null)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Green;
                        System.Console.WriteLine($"  ⚔️  Captured: {move.CapturedPiece.GetName()} (gained {GetPieceValue(move.CapturedPiece.Type)} points)");
                        System.Console.ResetColor();
                    }

                    if (move.PromotionPiece != null)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        System.Console.WriteLine($"  👑 Promoted to: {move.PromotionPiece.Value}");
                        System.Console.ResetColor();
                    }

                    if (move.IsCastling)
                    {
                        System.Console.ForegroundColor = ConsoleColor.Cyan;
                        System.Console.WriteLine($"  🏰 Castled - King is safer now!");
                        System.Console.ResetColor();
                    }

                    if (_board.IsInCheck(PieceColor.Black))
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
                _board = new Board();
                System.Console.WriteLine("\n✓ New game started!");
                System.Console.WriteLine($"Difficulty: {_analyzer.Difficulty}");
                System.Console.WriteLine($"Analytics: {(_showAnalytics ? "ON" : "OFF")}");
                return true;

            case "yolo":
                await MakeStockfishMoveForWhite();
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
        var whiteValidMoves = _board.GetValidMoves(PieceColor.White);
        var blackValidMoves = _board.GetValidMoves(PieceColor.Black);
        
        System.Console.WriteLine($"   Legal moves: You have {whiteValidMoves.Count}, Computer has {blackValidMoves.Count}");
        
        // Show move count
        int moveNumber = (_board.MoveHistory.Count / 2) + 1;
        System.Console.WriteLine($"   Move #{moveNumber} completed");
    }

    private int GetPieceValue(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 3,
            PieceType.Bishop => 3,
            PieceType.Rook => 5,
            PieceType.Queen => 9,
            PieceType.King => 0,
            _ => 0
        };
    }

    private void ShowMoveHistory()
    {
        if (_board.MoveHistory.Count == 0)
            return;

        System.Console.Write("\n📜 Moves: ");
        for (int i = 0; i < _board.MoveHistory.Count; i += 2)
        {
            int moveNum = (i / 2) + 1;
            string white = _board.MoveHistory[i].ToAlgebraic();
            string black = i + 1 < _board.MoveHistory.Count ? _board.MoveHistory[i + 1].ToAlgebraic() : "";
            
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
        System.Console.WriteLine("  Nf3         - Knight to f3");
        System.Console.WriteLine("  Bxc4        - Bishop captures on c4");
        System.Console.WriteLine("  exd5        - Pawn on e-file captures on d5");
        System.Console.WriteLine("  O-O         - Kingside castling");
        System.Console.WriteLine("  O-O-O       - Queenside castling");
        System.Console.WriteLine("  e8=Q        - Pawn promotion to Queen");
        System.Console.WriteLine("  Nbd2        - Knight from b-file to d2 (disambiguation)");
        System.Console.WriteLine();
        System.Console.WriteLine("  (Also accepts coordinate notation: e2e4, g1f3, e7e8q)");
        System.Console.WriteLine();
        System.Console.WriteLine("COMMANDS:");
        System.Console.WriteLine("  show/s      - 👀 Show the board (peeking!)");
        System.Console.WriteLine("  help/h/?    - Show this help");
        System.Console.WriteLine("  moves       - Show move history");
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
