namespace Chess.AI;

public class AnalysisResult
{
    public string? BestMove { get; set; }
    public double Evaluation { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}
