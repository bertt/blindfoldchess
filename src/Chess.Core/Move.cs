namespace Chess.Core;

public class Move
{
    public Position From { get; set; }
    public Position To { get; set; }
    public Piece? CapturedPiece { get; set; }
    public bool IsCastling { get; set; }
    public bool IsEnPassant { get; set; }
    public PieceType? PromotionPiece { get; set; }

    public Move(Position from, Position to)
    {
        From = from;
        To = to;
    }

    public Move(string algebraic)
    {
        // Parse moves like "e2e4", "e2-e4", "e4"
        algebraic = algebraic.Replace("-", "").Replace("x", "").Trim().ToLower();
        
        // Handle castling
        if (algebraic == "o-o" || algebraic == "oo" || algebraic == "0-0")
        {
            IsCastling = true;
            From = new Position(0, 4); // Will be adjusted based on color
            To = new Position(0, 6);
            return;
        }
        if (algebraic == "o-o-o" || algebraic == "ooo" || algebraic == "0-0-0")
        {
            IsCastling = true;
            From = new Position(0, 4);
            To = new Position(0, 2);
            return;
        }

        // Parse promotion (e.g., "e7e8q")
        if (algebraic.Length == 5)
        {
            PromotionPiece = algebraic[4] switch
            {
                'q' => PieceType.Queen,
                'r' => PieceType.Rook,
                'b' => PieceType.Bishop,
                'n' => PieceType.Knight,
                _ => null
            };
            algebraic = algebraic.Substring(0, 4);
        }

        if (algebraic.Length == 4)
        {
            From = new Position(algebraic.Substring(0, 2));
            To = new Position(algebraic.Substring(2, 2));
        }
        else
        {
            throw new ArgumentException($"Invalid move notation: {algebraic}");
        }
    }

    public string ToAlgebraic()
    {
        if (IsCastling)
        {
            return To.Col == 6 ? "O-O" : "O-O-O";
        }

        string move = $"{From.ToAlgebraic()}{To.ToAlgebraic()}";
        if (PromotionPiece.HasValue)
        {
            move += PromotionPiece.Value.ToString()[0].ToString().ToLower();
        }
        return move;
    }

    public override string ToString()
    {
        return ToAlgebraic();
    }
}
