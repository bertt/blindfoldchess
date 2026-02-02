namespace Chess.Core;

public class Board
{
    private Piece?[,] _squares = new Piece?[8, 8];
    public PieceColor CurrentTurn { get; private set; } = PieceColor.White;
    public List<Move> MoveHistory { get; private set; } = new();
    public Position? EnPassantTarget { get; private set; }

    public Board()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        // Black pieces
        _squares[7, 0] = new Piece(PieceType.Rook, PieceColor.Black);
        _squares[7, 1] = new Piece(PieceType.Knight, PieceColor.Black);
        _squares[7, 2] = new Piece(PieceType.Bishop, PieceColor.Black);
        _squares[7, 3] = new Piece(PieceType.Queen, PieceColor.Black);
        _squares[7, 4] = new Piece(PieceType.King, PieceColor.Black);
        _squares[7, 5] = new Piece(PieceType.Bishop, PieceColor.Black);
        _squares[7, 6] = new Piece(PieceType.Knight, PieceColor.Black);
        _squares[7, 7] = new Piece(PieceType.Rook, PieceColor.Black);

        for (int i = 0; i < 8; i++)
        {
            _squares[6, i] = new Piece(PieceType.Pawn, PieceColor.Black);
        }

        // White pieces
        _squares[0, 0] = new Piece(PieceType.Rook, PieceColor.White);
        _squares[0, 1] = new Piece(PieceType.Knight, PieceColor.White);
        _squares[0, 2] = new Piece(PieceType.Bishop, PieceColor.White);
        _squares[0, 3] = new Piece(PieceType.Queen, PieceColor.White);
        _squares[0, 4] = new Piece(PieceType.King, PieceColor.White);
        _squares[0, 5] = new Piece(PieceType.Bishop, PieceColor.White);
        _squares[0, 6] = new Piece(PieceType.Knight, PieceColor.White);
        _squares[0, 7] = new Piece(PieceType.Rook, PieceColor.White);

        for (int i = 0; i < 8; i++)
        {
            _squares[1, i] = new Piece(PieceType.Pawn, PieceColor.White);
        }
    }

    public Piece? GetPiece(Position pos)
    {
        if (!pos.IsValid()) return null;
        return _squares[pos.Row, pos.Col];
    }

    public Piece? GetPiece(int row, int col)
    {
        if (row < 0 || row > 7 || col < 0 || col > 7) return null;
        return _squares[row, col];
    }

    private void SetPiece(Position pos, Piece? piece)
    {
        if (pos.IsValid())
        {
            _squares[pos.Row, pos.Col] = piece;
        }
    }

    public bool MakeMove(Move move)
    {
        if (!IsValidMove(move, CurrentTurn))
            return false;

        var piece = GetPiece(move.From);
        if (piece == null) return false;

        // Handle en passant
        if (move.IsEnPassant)
        {
            var capturePos = new Position(move.From.Row, move.To.Col);
            move.CapturedPiece = GetPiece(capturePos);
            SetPiece(capturePos, null);
        }
        else
        {
            move.CapturedPiece = GetPiece(move.To);
        }

        // Handle castling
        if (move.IsCastling)
        {
            // Move king
            SetPiece(move.To, piece);
            SetPiece(move.From, null);
            piece.HasMoved = true;

            // Move rook
            if (move.To.Col == 6) // Kingside
            {
                var rook = GetPiece(move.From.Row, 7);
                SetPiece(new Position(move.From.Row, 5), rook);
                SetPiece(new Position(move.From.Row, 7), null);
                if (rook != null) rook.HasMoved = true;
            }
            else // Queenside
            {
                var rook = GetPiece(move.From.Row, 0);
                SetPiece(new Position(move.From.Row, 3), rook);
                SetPiece(new Position(move.From.Row, 0), null);
                if (rook != null) rook.HasMoved = true;
            }
        }
        else
        {
            // Normal move
            SetPiece(move.To, piece);
            SetPiece(move.From, null);
            piece.HasMoved = true;

            // Handle promotion
            if (move.PromotionPiece.HasValue)
            {
                SetPiece(move.To, new Piece(move.PromotionPiece.Value, piece.Color));
            }
        }

        // Set en passant target for next turn
        EnPassantTarget = null;
        if (piece.Type == PieceType.Pawn && Math.Abs(move.From.Row - move.To.Row) == 2)
        {
            EnPassantTarget = new Position((move.From.Row + move.To.Row) / 2, move.From.Col);
        }

        MoveHistory.Add(move);
        CurrentTurn = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        return true;
    }

    public bool IsValidMove(Move move, PieceColor color)
    {
        var piece = GetPiece(move.From);
        if (piece == null || piece.Color != color)
            return false;

        // Check if target is occupied by own piece
        var target = GetPiece(move.To);
        if (target != null && target.Color == color)
            return false;

        // Handle castling
        if (move.IsCastling)
            return CanCastle(color, move.To.Col == 6);

        // Check piece-specific movement rules
        if (!IsValidPieceMove(piece, move))
            return false;

        // Check if path is clear (for sliding pieces)
        if (!IsPathClear(move.From, move.To, piece.Type))
            return false;

        // Simulate move and check if king is in check
        if (WouldBeInCheck(move, color))
            return false;

        return true;
    }

    private bool IsValidPieceMove(Piece piece, Move move)
    {
        int rowDiff = move.To.Row - move.From.Row;
        int colDiff = move.To.Col - move.From.Col;
        int rowDir = piece.Color == PieceColor.White ? 1 : -1;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                // Forward move
                if (colDiff == 0)
                {
                    if (rowDiff == rowDir && GetPiece(move.To) == null)
                        return true;
                    if (rowDiff == 2 * rowDir && !piece.HasMoved && 
                        GetPiece(move.To) == null && 
                        GetPiece(new Position(move.From.Row + rowDir, move.From.Col)) == null)
                        return true;
                }
                // Capture
                else if (Math.Abs(colDiff) == 1 && rowDiff == rowDir)
                {
                    var target = GetPiece(move.To);
                    if (target != null && target.Color != piece.Color)
                        return true;
                    
                    // En passant
                    if (EnPassantTarget != null && move.To.Equals(EnPassantTarget))
                    {
                        move.IsEnPassant = true;
                        return true;
                    }
                }
                return false;

            case PieceType.Knight:
                return (Math.Abs(rowDiff) == 2 && Math.Abs(colDiff) == 1) ||
                       (Math.Abs(rowDiff) == 1 && Math.Abs(colDiff) == 2);

            case PieceType.Bishop:
                return Math.Abs(rowDiff) == Math.Abs(colDiff);

            case PieceType.Rook:
                return rowDiff == 0 || colDiff == 0;

            case PieceType.Queen:
                return rowDiff == 0 || colDiff == 0 || Math.Abs(rowDiff) == Math.Abs(colDiff);

            case PieceType.King:
                return Math.Abs(rowDiff) <= 1 && Math.Abs(colDiff) <= 1;

            default:
                return false;
        }
    }

    private bool IsPathClear(Position from, Position to, PieceType pieceType)
    {
        // Knights jump over pieces
        if (pieceType == PieceType.Knight)
            return true;

        int rowStep = to.Row > from.Row ? 1 : (to.Row < from.Row ? -1 : 0);
        int colStep = to.Col > from.Col ? 1 : (to.Col < from.Col ? -1 : 0);

        int row = from.Row + rowStep;
        int col = from.Col + colStep;

        while (row != to.Row || col != to.Col)
        {
            if (GetPiece(row, col) != null)
                return false;
            row += rowStep;
            col += colStep;
        }

        return true;
    }

    private bool CanCastle(PieceColor color, bool kingside)
    {
        int row = color == PieceColor.White ? 0 : 7;
        var king = GetPiece(row, 4);
        
        if (king == null || king.Type != PieceType.King || king.HasMoved)
            return false;

        if (IsInCheck(color))
            return false;

        if (kingside)
        {
            var rook = GetPiece(row, 7);
            if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
                return false;

            // Check if squares are empty and not under attack
            for (int col = 5; col <= 6; col++)
            {
                if (GetPiece(row, col) != null)
                    return false;
                if (IsSquareUnderAttack(new Position(row, col), color))
                    return false;
            }
        }
        else
        {
            var rook = GetPiece(row, 0);
            if (rook == null || rook.Type != PieceType.Rook || rook.HasMoved)
                return false;

            // Check if squares are empty
            for (int col = 1; col <= 3; col++)
            {
                if (GetPiece(row, col) != null)
                    return false;
            }
            // Check if squares king passes through are not under attack
            for (int col = 2; col <= 3; col++)
            {
                if (IsSquareUnderAttack(new Position(row, col), color))
                    return false;
            }
        }

        return true;
    }

    private bool WouldBeInCheck(Move move, PieceColor color)
    {
        // Make temporary move
        var piece = GetPiece(move.From);
        var captured = GetPiece(move.To);
        var originalEnPassant = EnPassantTarget;
        
        SetPiece(move.To, piece);
        SetPiece(move.From, null);

        bool inCheck = IsInCheck(color);

        // Undo move
        SetPiece(move.From, piece);
        SetPiece(move.To, captured);
        EnPassantTarget = originalEnPassant;

        return inCheck;
    }

    public bool IsInCheck(PieceColor color)
    {
        var kingPos = FindKing(color);
        if (kingPos == null) return false;
        return IsSquareUnderAttack(kingPos, color);
    }

    private bool IsSquareUnderAttack(Position pos, PieceColor defenderColor)
    {
        PieceColor attackerColor = defenderColor == PieceColor.White ? PieceColor.Black : PieceColor.White;

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = GetPiece(row, col);
                if (piece != null && piece.Color == attackerColor)
                {
                    var move = new Move(new Position(row, col), pos);
                    if (IsValidPieceMove(piece, move) && IsPathClear(move.From, move.To, piece.Type))
                        return true;
                }
            }
        }

        return false;
    }

    private Position? FindKing(PieceColor color)
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = GetPiece(row, col);
                if (piece != null && piece.Type == PieceType.King && piece.Color == color)
                    return new Position(row, col);
            }
        }
        return null;
    }

    public bool IsCheckmate(PieceColor color)
    {
        if (!IsInCheck(color))
            return false;

        return !HasAnyValidMove(color);
    }

    public bool IsStalemate(PieceColor color)
    {
        if (IsInCheck(color))
            return false;

        return !HasAnyValidMove(color);
    }

    private bool HasAnyValidMove(PieceColor color)
    {
        for (int fromRow = 0; fromRow < 8; fromRow++)
        {
            for (int fromCol = 0; fromCol < 8; fromCol++)
            {
                var piece = GetPiece(fromRow, fromCol);
                if (piece == null || piece.Color != color)
                    continue;

                for (int toRow = 0; toRow < 8; toRow++)
                {
                    for (int toCol = 0; toCol < 8; toCol++)
                    {
                        var move = new Move(new Position(fromRow, fromCol), new Position(toRow, toCol));
                        if (IsValidMove(move, color))
                            return true;
                    }
                }

                // Check castling
                if (piece.Type == PieceType.King)
                {
                    var kingsideCastle = new Move(new Position(fromRow, fromCol), new Position(fromRow, 6)) { IsCastling = true };
                    var queensideCastle = new Move(new Position(fromRow, fromCol), new Position(fromRow, 2)) { IsCastling = true };
                    if (IsValidMove(kingsideCastle, color) || IsValidMove(queensideCastle, color))
                        return true;
                }
            }
        }

        return false;
    }

    public List<Move> GetValidMoves(PieceColor color)
    {
        var validMoves = new List<Move>();

        for (int fromRow = 0; fromRow < 8; fromRow++)
        {
            for (int fromCol = 0; fromCol < 8; fromCol++)
            {
                var piece = GetPiece(fromRow, fromCol);
                if (piece == null || piece.Color != color)
                    continue;

                for (int toRow = 0; toRow < 8; toRow++)
                {
                    for (int toCol = 0; toCol < 8; toCol++)
                    {
                        var move = new Move(new Position(fromRow, fromCol), new Position(toRow, toCol));
                        if (IsValidMove(move, color))
                            validMoves.Add(move);
                    }
                }

                // Add castling moves
                if (piece.Type == PieceType.King)
                {
                    var kingsideCastle = new Move(new Position(fromRow, fromCol), new Position(fromRow, 6)) { IsCastling = true };
                    var queensideCastle = new Move(new Position(fromRow, fromCol), new Position(fromRow, 2)) { IsCastling = true };
                    if (IsValidMove(kingsideCastle, color))
                        validMoves.Add(kingsideCastle);
                    if (IsValidMove(queensideCastle, color))
                        validMoves.Add(queensideCastle);
                }
            }
        }

        return validMoves;
    }

    public string ToFen()
    {
        var fen = "";
        
        for (int row = 7; row >= 0; row--)
        {
            int emptyCount = 0;
            for (int col = 0; col < 8; col++)
            {
                var piece = GetPiece(row, col);
                if (piece == null)
                {
                    emptyCount++;
                }
                else
                {
                    if (emptyCount > 0)
                    {
                        fen += emptyCount;
                        emptyCount = 0;
                    }
                    fen += piece.ToFenChar();
                }
            }
            if (emptyCount > 0)
                fen += emptyCount;
            
            if (row > 0)
                fen += "/";
        }

        fen += CurrentTurn == PieceColor.White ? " w " : " b ";
        
        // Castling availability
        string castling = "";
        var whiteKing = GetPiece(0, 4);
        var blackKing = GetPiece(7, 4);
        
        if (whiteKing != null && !whiteKing.HasMoved)
        {
            var whiteKingsideRook = GetPiece(0, 7);
            var whiteQueensideRook = GetPiece(0, 0);
            if (whiteKingsideRook != null && !whiteKingsideRook.HasMoved)
                castling += "K";
            if (whiteQueensideRook != null && !whiteQueensideRook.HasMoved)
                castling += "Q";
        }
        
        if (blackKing != null && !blackKing.HasMoved)
        {
            var blackKingsideRook = GetPiece(7, 7);
            var blackQueensideRook = GetPiece(7, 0);
            if (blackKingsideRook != null && !blackKingsideRook.HasMoved)
                castling += "k";
            if (blackQueensideRook != null && !blackQueensideRook.HasMoved)
                castling += "q";
        }
        
        fen += (castling.Length > 0 ? castling : "-") + " ";
        
        // En passant target - NOTE: Some chess APIs don't handle en passant correctly,
        // so we use "-" instead. This is still a valid FEN, just without en passant info.
        fen += "- ";
        
        // Halfmove and fullmove (simplified for now)
        fen += "0 " + (MoveHistory.Count / 2 + 1);
        
        return fen;
    }

    public int GetMaterialValue(PieceColor color)
    {
        int value = 0;
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = GetPiece(row, col);
                if (piece != null && piece.Color == color)
                {
                    value += piece.Type switch
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
            }
        }
        return value;
    }

    public string GetMoveSAN(Move move)
    {
        // Get the base SAN
        string san = move.ToSAN(this);

        // Make a temporary move to check if it results in check/checkmate
        var tempBoard = CloneForSimulation();
        tempBoard.MakeMove(new Move(move.From, move.To) 
        { 
            CapturedPiece = move.CapturedPiece,
            IsCastling = move.IsCastling,
            IsEnPassant = move.IsEnPassant,
            PromotionPiece = move.PromotionPiece
        });

        var opponentColor = CurrentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        
        if (tempBoard.IsCheckmate(opponentColor))
            san += "#";
        else if (tempBoard.IsInCheck(opponentColor))
            san += "+";

        return san;
    }

    private Board CloneForSimulation()
    {
        var clone = new Board();
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var piece = GetPiece(row, col);
                if (piece != null)
                {
                    clone._squares[row, col] = new Piece(piece.Type, piece.Color) { HasMoved = piece.HasMoved };
                }
                else
                {
                    clone._squares[row, col] = null;
                }
            }
        }
        clone.CurrentTurn = CurrentTurn;
        clone.EnPassantTarget = EnPassantTarget;
        return clone;
    }
}
