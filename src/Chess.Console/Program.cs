using Chess.Core;
using Chess.AI;

namespace Chess.Console;

class Program
{
    public static readonly string Version = typeof(Program).Assembly.GetName().Version?.ToString(3) ?? "0.1.0";
    
    static async Task Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        System.Console.WriteLine("╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║   Blindfold Chess - Train Your Vision      ║");
        System.Console.WriteLine($"║              Version {Version,-22} ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("Goal: Learn blindfold chess! The board is NOT automatically shown.");
        System.Console.WriteLine("Type 'help' for commands, 'show' to see the board.");
        System.Console.WriteLine();

        // Check Copilot CLI status
        await CheckCopilotStatus();
        System.Console.WriteLine();

        var game = new ChessGame();
        await game.Run();
    }

    static async Task CheckCopilotStatus()
    {
        System.Console.Write("Checking GitHub Copilot CLI... ");
        
        try
        {
            // On Windows, try both copilot and copilot.cmd
            var fileName = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows) ? "copilot.cmd" : "copilot";
            
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                var version = output.Trim().Split('\n')[0].Trim();
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine($"✓ Installed ({version})");
                System.Console.ResetColor();
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("⚠ Not responding correctly");
                System.Console.ResetColor();
                System.Console.WriteLine("  Try: copilot auth");
                System.Console.WriteLine("  The app will use basic material evaluation as fallback.");
            }
        }
        catch (Exception)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("⚠ Not found");
            System.Console.ResetColor();
            System.Console.WriteLine("  Install with: npm install -g copilot");
            System.Console.WriteLine("  Then authenticate with: copilot auth");
            System.Console.WriteLine("  The app will use basic material evaluation as fallback.");
        }
    }
}

class ChessGame
{
    private Board _board;
    private CopilotChessAnalyzer _analyzer;
    private bool _isRunning = true;

    public ChessGame()
    {
        _board = new Board();
        _analyzer = new CopilotChessAnalyzer();
    }

    public async Task Run()
    {
        try
        {
            System.Console.WriteLine($"New game started! You play WHITE (♙), computer plays BLACK (♟)");
            System.Console.WriteLine($"Difficulty: {_analyzer.Difficulty}");
            System.Console.WriteLine($"AI Model: {_analyzer.Model}");
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
        var input = System.Console.ReadLine()?.Trim().ToLower();

        if (string.IsNullOrEmpty(input))
            return;

        // Handle commands
        if (await HandleCommand(input))
            return;

        // Try to parse and make move
        try
        {
            var move = new Move(input);
            
            // Adjust castling for white
            if (move.IsCastling)
            {
                move.From = new Position(0, 4);
                move.To = new Position(0, move.To.Col == 6 ? 6 : 2);
            }

            if (_board.MakeMove(move))
            {
                var piece = _board.GetPiece(move.To);
                string pieceName = piece?.GetName() ?? "?";
                
                System.Console.WriteLine($"✓ Move played: {move.ToAlgebraic()} ({pieceName} to {move.To.ToAlgebraic()})");
                
                if (move.CapturedPiece != null)
                {
                    System.Console.WriteLine($"  Captured: {move.CapturedPiece.GetName()}");
                }

                // Show analysis
                var analysis = await _analyzer.AnalyzePosition(_board);
                ShowAnalysis(analysis);
                ShowMoveHistory();
            }
            else
            {
                System.Console.WriteLine("❌ Invalid move! Try again.");
                System.Console.WriteLine("   Use: e2e4, e7e8q (promotion), o-o (kingside castle), o-o-o (queenside castle)");
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
                _board.MakeMove(move);
                var piece = _board.GetPiece(move.To);
                string pieceName = piece?.GetName() ?? "?";
                
                System.Console.WriteLine($"💻 Computer move: {move.ToAlgebraic()} ({pieceName} to {move.To.ToAlgebraic()})");
                
                if (move.CapturedPiece != null)
                {
                    System.Console.WriteLine($"   Captured: {move.CapturedPiece.GetName()}");
                }

                // Show analysis
                var analysis = await _analyzer.AnalyzePosition(_board);
                ShowAnalysis(analysis);
                ShowMoveHistory();
            }
            else
            {
                System.Console.WriteLine("❌ Computer cannot make a move!");
                _isRunning = false;
            }
        }
        catch (TimeoutException)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("\n❌ ERROR: GitHub Copilot timeout!");
            System.Console.ResetColor();
            System.Console.WriteLine("   Copilot did not respond within 15 seconds.");
            System.Console.WriteLine("   Please check:");
            System.Console.WriteLine("   1. Your internet connection");
            System.Console.WriteLine("   2. Copilot CLI is running: copilot --version");
            System.Console.WriteLine("   3. You are authenticated: copilot auth");
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
        catch (Exception ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            System.Console.ResetColor();
            System.Console.WriteLine("   GitHub Copilot is not working correctly.");
            System.Console.WriteLine("   Make sure Copilot CLI is installed and authenticated.");
            System.Console.WriteLine("   Install: npm install -g copilot");
            System.Console.WriteLine("   Auth: copilot auth");
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

            case "model":
            case "m":
                await ShowModelMenu();
                return true;

            case "version":
            case "v":
                ShowVersion();
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

    private async Task ShowModelMenu()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║           AI MODEL SELECTION               ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine($"Current: {_analyzer.Model}");
        System.Console.WriteLine();
        System.Console.WriteLine("AVAILABLE MODELS:");
        System.Console.WriteLine();
        System.Console.WriteLine("1. gpt-4o-mini (Default)");
        System.Console.WriteLine("   ⚡ Fastest | 💰 Cheapest | 🎯 Good chess strength");
        System.Console.WriteLine("   Best for: Quick games, practice, blindfold training");
        System.Console.WriteLine();
        System.Console.WriteLine("2. gpt-4o");
        System.Console.WriteLine("   ⚖️  Balanced | 💰💰 Moderate cost | 🎯🎯 Strong chess");
        System.Console.WriteLine("   Best for: Challenging games, learning tactics");
        System.Console.WriteLine();
        System.Console.WriteLine("3. claude-sonnet-4.5");
        System.Console.WriteLine("   🧠 Strategic | 💰💰 Moderate cost | 🎯🎯 Creative play");
        System.Console.WriteLine("   Best for: Positional chess, varied openings");
        System.Console.WriteLine();
        System.Console.WriteLine("4. gpt-4.1");
        System.Console.WriteLine("   🚀 Fast | 💰 Low cost | 🎯 Decent strength");
        System.Console.WriteLine("   Best for: Quick practice games");
        System.Console.WriteLine();
        System.Console.WriteLine("COST/BENEFIT SUMMARY:");
        System.Console.WriteLine("  Speed:    4.1 > 4o-mini > 4o ≈ claude");
        System.Console.WriteLine("  Cost:     4.1 < 4o-mini < 4o ≈ claude");
        System.Console.WriteLine("  Strength: 4o > claude > 4o-mini > 4.1");
        System.Console.WriteLine();
        System.Console.Write("Enter model number (1-4) or name: ");
        
        var input = System.Console.ReadLine()?.Trim().ToLower();
        
        string? newModel = input switch
        {
            "1" or "gpt-4o-mini" or "mini" => "gpt-4o-mini",
            "2" or "gpt-4o" or "4o" => "gpt-4o",
            "3" or "claude-sonnet-4.5" or "claude" or "sonnet" => "claude-sonnet-4.5",
            "4" or "gpt-4.1" or "4.1" => "gpt-4.1",
            _ => null
        };

        if (newModel != null)
        {
            System.Console.Write($"\n⏳ Switching to {newModel}... ");
            try
            {
                await _analyzer.ChangeModelAsync(newModel);
                System.Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("✓ Success!");
                System.Console.ResetColor();
                System.Console.WriteLine($"Now using: {_analyzer.Model}");
            }
            catch (Exception ex)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"✗ Failed: {ex.Message}");
                System.Console.ResetColor();
            }
        }
        else
        {
            System.Console.WriteLine("❌ Invalid selection. Model unchanged.");
        }
        System.Console.WriteLine();
    }

    private void ShowBoard()
    {
        System.Console.WriteLine(_board.ToDisplayString());
    }

    private void ShowAnalysis(AnalysisResult analysis)
    {
        System.Console.WriteLine("\n📊 Analysis:");
        
        if (analysis.Details.ContainsKey("WhiteMaterial") && analysis.Details.ContainsKey("BlackMaterial"))
        {
            int whiteMat = int.Parse(analysis.Details["WhiteMaterial"]);
            int blackMat = int.Parse(analysis.Details["BlackMaterial"]);
            int diff = whiteMat - blackMat;
            
            System.Console.WriteLine($"   Material: White {whiteMat} - Black {blackMat} (difference: {(diff >= 0 ? "+" : "")}{diff})");
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
        System.Console.WriteLine("\n🔍 DEBUG - Last Copilot Interaction:");
        System.Console.WriteLine("═══════════════════════════════════════════");
        
        if (string.IsNullOrEmpty(_analyzer.LastPrompt))
        {
            System.Console.WriteLine("No Copilot interaction yet.");
            return;
        }

        System.Console.WriteLine("\n📤 PROMPT:");
        System.Console.WriteLine("-------------------------------------------");
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(_analyzer.LastPrompt);
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
        System.Console.WriteLine($"  AI Model: {_analyzer.Model}");
        System.Console.WriteLine($"  Difficulty: {_analyzer.Difficulty}");
        System.Console.WriteLine();
        System.Console.WriteLine("  GitHub: https://github.com/bertt/blindfoldchess");
        System.Console.WriteLine();
    }

    private void ShowHelp()
    {
        System.Console.WriteLine("\n╔════════════════════════════════════════════╗");
        System.Console.WriteLine("║              COMMANDS                      ║");
        System.Console.WriteLine("╚════════════════════════════════════════════╝");
        System.Console.WriteLine();
        System.Console.WriteLine("MOVES:");
        System.Console.WriteLine("  e2e4        - Move piece from e2 to e4");
        System.Console.WriteLine("  e7e8q       - Pawn promotion to Queen");
        System.Console.WriteLine("  o-o         - Kingside castling");
        System.Console.WriteLine("  o-o-o       - Queenside castling");
        System.Console.WriteLine();
        System.Console.WriteLine("COMMANDS:");
        System.Console.WriteLine("  show/s      - 👀 Show the board (peeking!)");
        System.Console.WriteLine("  help/h/?    - Show this help");
        System.Console.WriteLine("  moves       - Show move history");
        System.Console.WriteLine("  analyze/a   - Analyze current position");
        System.Console.WriteLine("  debug/d     - 🔍 Show last AI prompt & response");
        System.Console.WriteLine("  level/l     - Change difficulty level");
        System.Console.WriteLine("  model/m     - 🤖 Change AI model");
        System.Console.WriteLine("  version/v   - Show version information");
        System.Console.WriteLine("  new         - Start new game");
        System.Console.WriteLine("  quit/q      - Exit");
        System.Console.WriteLine();
        System.Console.WriteLine("PIECES:");
        System.Console.WriteLine("  White: ♔ King  ♕ Queen  ♖ Rook  ♗ Bishop  ♘ Knight  ♙ Pawn");
        System.Console.WriteLine("  Black: ♚ King  ♛ Queen  ♜ Rook  ♝ Bishop  ♞ Knight  ♟ Pawn");
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
