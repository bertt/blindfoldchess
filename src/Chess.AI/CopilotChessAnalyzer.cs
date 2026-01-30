using GitHub.Copilot.SDK;
using Chess.Core;

namespace Chess.AI;

public class CopilotChessAnalyzer : IAsyncDisposable
{
    private readonly CopilotClient _client;
    private CopilotSession? _session;
    private bool _isInitialized = false;
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Intermediate;
    public string Model { get; set; } = "gpt-4o-mini";
    
    // Debug information
    public string? LastPrompt { get; private set; }
    public string? LastResponse { get; private set; }

    public CopilotChessAnalyzer()
    {
        _client = new CopilotClient();
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            await _client.StartAsync();
            _session = await _client.CreateSessionAsync(new SessionConfig
            {
                Model = Model
            });
            _isInitialized = true;
        }
    }

    public async Task ChangeModelAsync(string newModel)
    {
        if (Model == newModel && _isInitialized)
            return; // Already using this model

        // Dispose old session
        if (_session != null)
        {
            await _session.DisposeAsync();
            _session = null;
        }

        Model = newModel;
        _isInitialized = false;

        // Reinitialize with new model
        await EnsureInitializedAsync();
    }

    private async Task<string> AskCopilotAsync(string prompt, int timeoutSeconds = 10)
    {
        await EnsureInitializedAsync();
        
        // Store prompt for debugging
        LastPrompt = prompt;
        
        var responseText = "";
        var done = new TaskCompletionSource();
        IDisposable? subscription = null;

        subscription = _session!.On(evt =>
        {
            if (evt is AssistantMessageEvent msg)
            {
                responseText = msg.Data.Content;
            }
            else if (evt is SessionIdleEvent)
            {
                subscription?.Dispose();
                done.TrySetResult();
            }
        });

        try
        {
            await _session.SendAsync(new MessageOptions { Prompt = prompt });
            
            // Add timeout
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(timeoutSeconds));
            var completedTask = await Task.WhenAny(done.Task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("Copilot response timed out");
            }
            
            // Store response for debugging
            LastResponse = responseText;
            
            return responseText;
        }
        finally
        {
            subscription?.Dispose();
        }
    }

    public async Task<AnalysisResult> AnalyzePosition(Board board)
    {
        var result = new AnalysisResult();

        // Calculate basic metrics
        int whiteMaterial = board.GetMaterialValue(PieceColor.White);
        int blackMaterial = board.GetMaterialValue(PieceColor.Black);
        int materialDiff = whiteMaterial - blackMaterial;

        result.Details["WhiteMaterial"] = whiteMaterial.ToString();
        result.Details["BlackMaterial"] = blackMaterial.ToString();
        result.Details["MaterialDifference"] = materialDiff.ToString();

        // Check game state
        if (board.IsCheckmate(board.CurrentTurn))
        {
            result.Description = $"Checkmate! {(board.CurrentTurn == PieceColor.White ? "Black" : "White")} wins!";
            result.Evaluation = board.CurrentTurn == PieceColor.White ? -999 : 999;
            return result;
        }

        if (board.IsStalemate(board.CurrentTurn))
        {
            result.Description = "Stalemate! Draw.";
            result.Evaluation = 0;
            return result;
        }

        if (board.IsInCheck(board.CurrentTurn))
        {
            result.Details["Check"] = "Yes";
        }

        // Simple evaluation without Copilot for speed
        if (Math.Abs(materialDiff) < 2)
            result.Description = "Equal position";
        else if (materialDiff > 0)
            result.Description = materialDiff > 5 ? "Large advantage White" : "Slight advantage White";
        else
            result.Description = materialDiff < -5 ? "Large advantage Black" : "Slight advantage Black";
        
        result.Evaluation = materialDiff;

        return result;
    }

    public async Task<Move?> GetBestMove(Board board, PieceColor color)
    {
        var validMoves = board.GetValidMoves(color);
        
        if (validMoves.Count == 0)
            return null;

        string fen = board.ToFen();
        string movesStr = string.Join(", ", validMoves.Select(m => m.ToAlgebraic()));
        
        // Get move history for context
        var moveHistory = board.MoveHistory;
        string historyStr = moveHistory.Count > 0 
            ? string.Join(" ", moveHistory.TakeLast(6).Select(m => m.ToAlgebraic()))
            : "game start";

        // Check for threats and opportunities
        bool inCheck = board.IsInCheck(color);
        string checkInfo = inCheck ? " You are in CHECK!" : "";

        // Build context-aware prompts
        string prompt = Difficulty switch
        {
            DifficultyLevel.Beginner => 
                $@"You are a beginner player (rating 800). Play simple, safe chess.

Position (FEN): {fen}
Recent moves: {historyStr}{checkInfo}
Your valid moves: {movesStr}

Choose ONE move. Think about:
- Getting pieces out from starting position
- Controlling center squares (e4, d4, e5, d5)
- Protecting your king

Reply with ONLY the move (e.g., e2e4):",

            DifficultyLevel.Intermediate => 
                $@"You are an intermediate player (rating 1500). Play tactical, solid chess.

Position (FEN): {fen}
Recent moves: {historyStr}{checkInfo}
Your valid moves: {movesStr}

Analyze and choose the BEST move. Consider:
1. Captures - can you win material?
2. Threats - can you attack opponent's pieces?
3. Development - get knights and bishops out
4. Center control - fight for e4, d4, e5, d5
5. King safety - castle when possible
6. Piece coordination - connect your rooks

Look for tactical patterns: pins, forks, skewers, discovered attacks.
Avoid hanging pieces! Make sure your pieces are protected.

Reply with ONLY the move (e.g., e2e4):",

            DifficultyLevel.Advanced => 
                $@"You are a master player (rating 2200). Play strong, strategic chess.

Position (FEN): {fen}
Recent moves: {historyStr}{checkInfo}
Your valid moves: {movesStr}

Deep analysis required. Evaluate:
1. Material - calculate exact exchanges
2. Tactics - pins, forks, skewers, discovered attacks, deflection
3. King safety - analyze pawn shield, open files, attack potential
4. Pawn structure - weak squares, isolated pawns, passed pawns
5. Piece activity - which pieces are strong/weak?
6. Strategic plans - attack kingside, queenside, or center?
7. Endgame potential - is simplification good or bad?

Consider candidate moves and calculate 3-4 moves ahead.
Look for forcing moves: checks, captures, threats.

Reply with ONLY the best move (e.g., e2e4):",
            
            _ => $"Valid: {movesStr}. Reply one move:"
        };

        string response = await AskCopilotAsync(prompt, timeoutSeconds: 20);
        
        if (string.IsNullOrEmpty(response))
        {
            throw new Exception("Copilot returned empty response");
        }

        string moveStr = response.Trim().ToLower()
            .Replace("the best move is", "")
            .Replace("i recommend", "")
            .Replace("i suggest", "")
            .Replace("i would play", "")
            .Replace(":", "")
            .Replace(".", "")
            .Replace("!", "")
            .Trim();
        
        // Find matching move
        foreach (var move in validMoves)
        {
            string moveAlg = move.ToAlgebraic().ToLower();
            if (moveStr.Contains(moveAlg) || moveAlg.Contains(moveStr.Replace("-", "")))
            {
                return move;
            }
        }

        throw new Exception($"Could not parse Copilot move response: '{moveStr}'");
    }

    public async ValueTask DisposeAsync()
    {
        if (_session != null)
        {
            await _session.DisposeAsync();
        }
        if (_isInitialized)
        {
            await _client.StopAsync();
        }
        _client.Dispose();
    }
}
