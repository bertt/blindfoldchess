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

    public Move(string notation)
    {
        // This constructor handles long algebraic notation only (e2e4, e7e8q)
        // For SAN parsing with board context, use ParseSAN static method
        
        notation = notation.Replace("-", "").Replace("x", "").Trim().ToLower();
        
        // Handle castling
        if (notation == "o-o" || notation == "oo" || notation == "0-0")
        {
            IsCastling = true;
            From = new Position(0, 4); // Will be adjusted based on color
            To = new Position(0, 6);
            return;
        }
        if (notation == "o-o-o" || notation == "ooo" || notation == "0-0-0")
        {
            IsCastling = true;
            From = new Position(0, 4);
            To = new Position(0, 2);
            return;
        }

        // Parse promotion (e.g., "e7e8q")
        if (notation.Length == 5)
        {
            PromotionPiece = notation[4] switch
            {
                'q' => PieceType.Queen,
                'r' => PieceType.Rook,
                'b' => PieceType.Bishop,
                'n' => PieceType.Knight,
                _ => null
            };
            notation = notation.Substring(0, 4);
        }

        if (notation.Length == 4)
        {
            From = new Position(notation.Substring(0, 2));
            To = new Position(notation.Substring(2, 2));
        }
        else
        {
            throw new ArgumentException($"Invalid move notation: {notation}");
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

    public string ToSAN(Board board)
    {
        if (IsCastling)
        {
            return To.Col == 6 ? "O-O" : "O-O-O";
        }

        var piece = board.GetPiece(From);
        if (piece == null) return ToAlgebraic();

        string san = "";
        bool isCapture = CapturedPiece != null || IsEnPassant;

        // Add piece letter (omit for pawns)
        if (piece.Type != PieceType.Pawn)
        {
            san += GetPieceSymbol(piece.Type);

            // Check for disambiguation
            var disambiguation = GetDisambiguation(board, piece);
            san += disambiguation;
        }
        else if (isCapture)
        {
            // For pawn captures, add file of origin
            san += (char)('a' + From.Col);
        }

        // Add capture symbol
        if (isCapture)
        {
            san += "x";
        }

        // Add destination square
        san += To.ToAlgebraic();

        // Add promotion
        if (PromotionPiece.HasValue)
        {
            san += "=" + GetPieceSymbol(PromotionPiece.Value);
        }

        // Check for check/checkmate - will be added by caller
        return san;
    }

    private string GetDisambiguation(Board board, Piece piece)
    {
        var validMoves = board.GetValidMoves(piece.Color);
        var samePieceMoves = validMoves
            .Where(m => board.GetPiece(m.From)?.Type == piece.Type && 
                       m.To.Equals(To) && 
                       !m.From.Equals(From))
            .ToList();

        if (samePieceMoves.Count == 0)
            return "";

        bool sameFile = samePieceMoves.Any(m => m.From.Col == From.Col);
        bool sameRank = samePieceMoves.Any(m => m.From.Row == From.Row);

        if (!sameFile)
            return ((char)('a' + From.Col)).ToString();
        if (!sameRank)
            return (From.Row + 1).ToString();
        
        return From.ToAlgebraic();
    }

    private string GetPieceSymbol(PieceType type)
    {
        return type switch
        {
            PieceType.King => "K",
            PieceType.Queen => "Q",
            PieceType.Rook => "R",
            PieceType.Bishop => "B",
            PieceType.Knight => "N",
            _ => ""
        };
    }

    public override string ToString()
    {
        return ToAlgebraic();
    }

    public static Move? ParseSAN(string san, Board board)
    {
        san = san.Trim();
        
        // Remove check/checkmate symbols
        san = san.TrimEnd('+', '#');

        // Handle castling
        if (san == "O-O" || san == "o-o" || san == "0-0")
        {
            int row = board.CurrentTurn == PieceColor.White ? 0 : 7;
            return new Move(new Position(row, 4), new Position(row, 6)) { IsCastling = true };
        }
        if (san == "O-O-O" || san == "o-o-o" || san == "0-0-0")
        {
            int row = board.CurrentTurn == PieceColor.White ? 0 : 7;
            return new Move(new Position(row, 4), new Position(row, 2)) { IsCastling = true };
        }

        // Parse promotion
        PieceType? promotionPiece = null;
        if (san.Contains('='))
        {
            var parts = san.Split('=');
            san = parts[0];
            promotionPiece = parts[1].ToUpper()[0] switch
            {
                'Q' => PieceType.Queen,
                'R' => PieceType.Rook,
                'B' => PieceType.Bishop,
                'N' => PieceType.Knight,
                _ => null
            };
        }

        // Determine piece type
        PieceType pieceType = PieceType.Pawn;
        int startIndex = 0;
        if (char.IsUpper(san[0]))
        {
            pieceType = san[0] switch
            {
                'K' => PieceType.King,
                'Q' => PieceType.Queen,
                'R' => PieceType.Rook,
                'B' => PieceType.Bishop,
                'N' => PieceType.Knight,
                _ => PieceType.Pawn
            };
            startIndex = 1;
        }

        // Remove capture symbol
        bool isCapture = san.Contains('x');
        san = san.Replace("x", "");

        // Extract destination (last 2 characters)
        if (san.Length < startIndex + 2)
            return null;
        
        string destSquare = san.Substring(san.Length - 2);
        Position? toPos = null;
        try
        {
            toPos = new Position(destSquare);
        }
        catch
        {
            return null;
        }

        // Extract disambiguation (file, rank, or both)
        string disambiguation = san.Substring(startIndex, san.Length - startIndex - 2);
        
        int? fromFile = null;
        int? fromRank = null;
        
        foreach (char c in disambiguation)
        {
            if (c >= 'a' && c <= 'h')
                fromFile = c - 'a';
            else if (c >= '1' && c <= '8')
                fromRank = c - '1';
        }

        // Find the matching move from valid moves
        var validMoves = board.GetValidMoves(board.CurrentTurn);
        var candidates = validMoves.Where(m =>
        {
            var piece = board.GetPiece(m.From);
            if (piece == null || piece.Type != pieceType)
                return false;
            if (!m.To.Equals(toPos))
                return false;
            if (fromFile.HasValue && m.From.Col != fromFile.Value)
                return false;
            if (fromRank.HasValue && m.From.Row != fromRank.Value)
                return false;
            if (isCapture && m.CapturedPiece == null && !m.IsEnPassant)
                return false;
            return true;
        }).ToList();

        if (candidates.Count == 0)
            return null;

        var move = candidates[0];
        if (promotionPiece.HasValue)
            move.PromotionPiece = promotionPiece;

        return move;
    }
}
