using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Chess;

namespace Chess.AI;

public class ChessApiAnalyzer : IAsyncDisposable
{
    private static readonly HttpClient _httpClient = new();
    private const string ApiUrl = "https://chess-api.com/v1";
    
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Intermediate;
    
    // Debug information
    public string? LastRequest { get; private set; }
    public string? LastResponse { get; private set; }

    private int GetDepthForDifficulty()
    {
        return Difficulty switch
        {
            DifficultyLevel.Beginner => 10,      // ~1000 ELO
            DifficultyLevel.Intermediate => 12,  // ~1800 ELO
            DifficultyLevel.Advanced => 15,      // ~2400 ELO
            _ => 12
        };
    }

    private static string RemoveEnPassantFromFen(string fen)
    {
        // chess-api.com doesn't accept en passant targets in FEN
        // Convert: "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
        // To:      "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1"
        var parts = fen.Split(' ');
        if (parts.Length >= 4 && parts[3] != "-")
        {
            parts[3] = "-";
        }
        return string.Join(' ', parts);
    }

    public async Task<AnalysisResult> AnalyzePosition(ChessBoard board)
    {
        var result = new AnalysisResult();

        // Calculate basic metrics
        int whiteMaterial = CalculateMaterial(board, PieceColor.White);
        int blackMaterial = CalculateMaterial(board, PieceColor.Black);
        int materialDiff = whiteMaterial - blackMaterial;

        result.Details["WhiteMaterial"] = whiteMaterial.ToString();
        result.Details["BlackMaterial"] = blackMaterial.ToString();
        result.Details["MaterialDifference"] = materialDiff.ToString();

        // Check game state
        if (board.IsEndGame)
        {
            if (board.EndGame?.EndgameType == EndgameType.Checkmate)
            {
                result.Description = $"Checkmate! {board.EndGame.WonSide} wins!";
                result.Evaluation = board.EndGame.WonSide == PieceColor.White ? 999 : -999;
                return result;
            }
            else if (board.EndGame?.EndgameType == EndgameType.Stalemate)
            {
                result.Description = "Stalemate! Draw.";
                result.Evaluation = 0;
                return result;
            }
        }

        bool isWhiteTurn = board.Turn == PieceColor.White;
        if ((isWhiteTurn && board.WhiteKingChecked) || (!isWhiteTurn && board.BlackKingChecked))
        {
            result.Details["Check"] = "Yes";
        }

        // Get evaluation from Chess API
        try
        {
            string fen = RemoveEnPassantFromFen(board.ToFen());
            int depth = GetDepthForDifficulty();

            var request = new ChessApiRequest
            {
                Fen = fen,
                Depth = depth,
                MaxThinkingTime = 50
            };

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, request);
            response.EnsureSuccessStatusCode();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = await response.Content.ReadFromJsonAsync<ChessApiResponse>(options);
            
            if (apiResponse != null)
            {
                // Use API's eval score
                if (apiResponse.Eval.HasValue)
                {
                    result.Evaluation = apiResponse.Eval.Value;
                    
                    // Create our own description based on eval and depth
                    double eval = apiResponse.Eval.Value;
                    int responseDepth = apiResponse.Depth ?? depth;
                    
                    string positionAssessment;
                    if (Math.Abs(eval) < 0.5)
                        positionAssessment = "The game is balanced";
                    else if (eval > 3.0)
                        positionAssessment = "White is winning";
                    else if (eval > 1.0)
                        positionAssessment = "White has a clear advantage";
                    else if (eval > 0.5)
                        positionAssessment = "White has a slight advantage";
                    else if (eval < -3.0)
                        positionAssessment = "Black is winning";
                    else if (eval < -1.0)
                        positionAssessment = "Black has a clear advantage";
                    else
                        positionAssessment = "Black has a slight advantage";
                    
                    result.Description = $"{positionAssessment}. Eval: [{eval:F2}], Depth: {responseDepth}";
                }
                else
                {
                    result.Evaluation = materialDiff;
                    result.Description = GetMaterialDescription(materialDiff);
                }
            }
            else
            {
                // Fallback to material-based evaluation
                result.Description = GetMaterialDescription(materialDiff);
                result.Evaluation = materialDiff;
            }
        }
        catch
        {
            // Fallback to material-based evaluation if API fails
            result.Description = GetMaterialDescription(materialDiff);
            result.Evaluation = materialDiff;
        }

        return result;
    }

    private string GetMaterialDescription(int materialDiff)
    {
        if (Math.Abs(materialDiff) < 2)
            return "Equal position";
        else if (materialDiff > 0)
            return materialDiff > 5 ? "Large advantage White" : "Slight advantage White";
        else
            return materialDiff < -5 ? "Large advantage Black" : "Slight advantage Black";
    }

    public async Task<string?> GetBestMove(ChessBoard board, PieceColor color)
    {
        var validMoves = board.Moves();
        
        if (validMoves.Length == 0)
            return null;

        string fen = RemoveEnPassantFromFen(board.ToFen());
        int depth = GetDepthForDifficulty();

        var request = new ChessApiRequest
        {
            Fen = fen,
            Depth = depth,
            MaxThinkingTime = 50
        };

        LastRequest = System.Text.Json.JsonSerializer.Serialize(request);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, request);
            response.EnsureSuccessStatusCode();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var apiResponse = await response.Content.ReadFromJsonAsync<ChessApiResponse>(options);
            
            if (apiResponse == null)
            {
                throw new Exception("Chess API returned null response");
            }

            LastResponse = System.Text.Json.JsonSerializer.Serialize(apiResponse);

            // Parse the move from API response - prefer SAN for Gera.Chess
            string moveStr = apiResponse.San ?? apiResponse.Move ?? apiResponse.Lan ?? "";
            
            if (string.IsNullOrEmpty(moveStr))
            {
                throw new Exception($"Chess API did not return a valid move. Response: {LastResponse}");
            }

            // Validate move is legal (Gera.Chess will validate when we apply it)
            return moveStr;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Chess API request failed: {ex.Message}", ex);
        }
    }

    private int CalculateMaterial(ChessBoard board, PieceColor color)
    {
        int total = 0;
        // Use 0-based indexing: x=0-7 (columns a-h), y=0-7 (rows 1-8)
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var piece = board[x, y];
                if (piece != null && piece.Color == color)
                {
                    if (piece.Type == PieceType.Pawn) total += 1;
                    else if (piece.Type == PieceType.Knight) total += 3;
                    else if (piece.Type == PieceType.Bishop) total += 3;
                    else if (piece.Type == PieceType.Rook) total += 5;
                    else if (piece.Type == PieceType.Queen) total += 9;
                }
            }
        }
        return total;
    }

    public ValueTask DisposeAsync()
    {
        // HttpClient is static and shared, no disposal needed
        return ValueTask.CompletedTask;
    }

    private class ChessApiRequest
    {
        [JsonPropertyName("fen")]
        public string Fen { get; set; } = "";

        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("maxThinkingTime")]
        public int MaxThinkingTime { get; set; }
    }

    private class ChessApiResponse
    {
        [JsonPropertyName("move")]
        public string? Move { get; set; }

        [JsonPropertyName("lan")]
        public string? Lan { get; set; }

        [JsonPropertyName("san")]
        public string? San { get; set; }

        [JsonPropertyName("eval")]
        public double? Eval { get; set; }

        [JsonPropertyName("depth")]
        public int? Depth { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}

