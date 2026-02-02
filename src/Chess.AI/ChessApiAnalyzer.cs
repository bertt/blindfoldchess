using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Chess.Core;

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

        // Get evaluation from Chess API
        try
        {
            string fen = board.ToFen();
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

    public async Task<Move?> GetBestMove(Board board, PieceColor color)
    {
        var validMoves = board.GetValidMoves(color);
        
        if (validMoves.Count == 0)
            return null;

        string fen = board.ToFen();
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

            // Parse the move from API response - try multiple fields
            string moveStr = (apiResponse.Move ?? apiResponse.Lan ?? apiResponse.San ?? "").ToLower();
            
            if (string.IsNullOrEmpty(moveStr))
            {
                throw new Exception($"Chess API did not return a valid move. Response: {LastResponse}");
            }

            // Find matching move in valid moves
            foreach (var move in validMoves)
            {
                string moveAlg = move.ToAlgebraic().ToLower();
                
                // Direct match (e.g., "e2e4" == "e2e4")
                if (moveStr == moveAlg)
                {
                    return move;
                }
                
                // Match without hyphens for castling (e.g., "o-o" vs "oo")
                if (moveAlg.Replace("-", "") == moveStr.Replace("-", ""))
                {
                    return move;
                }
            }

            // If no match found, provide detailed error
            var validMovesList = string.Join(", ", validMoves.Select(m => m.ToAlgebraic()));
            throw new Exception($"Could not match API move '{moveStr}' to any valid move. Valid moves: {validMovesList}");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Chess API request failed: {ex.Message}", ex);
        }
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

