namespace Chess.Core;

public class Piece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public bool HasMoved { get; set; }

    public Piece(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
        HasMoved = false;
    }

    public char ToFenChar()
    {
        char c = Type switch
        {
            PieceType.Pawn => 'p',
            PieceType.Knight => 'n',
            PieceType.Bishop => 'b',
            PieceType.Rook => 'r',
            PieceType.Queen => 'q',
            PieceType.King => 'k',
            _ => '?'
        };
        return Color == PieceColor.White ? char.ToUpper(c) : c;
    }

    public char ToUnicode()
    {
        return (Type, Color) switch
        {
            (PieceType.King, PieceColor.White) => '♚',
            (PieceType.Queen, PieceColor.White) => '♛',
            (PieceType.Rook, PieceColor.White) => '♜',
            (PieceType.Bishop, PieceColor.White) => '♝',
            (PieceType.Knight, PieceColor.White) => '♞',
            (PieceType.Pawn, PieceColor.White) => '♙',
            (PieceType.King, PieceColor.Black) => '♔',
            (PieceType.Queen, PieceColor.Black) => '♕',
            (PieceType.Rook, PieceColor.Black) => '♖',
            (PieceType.Bishop, PieceColor.Black) => '♗',
            (PieceType.Knight, PieceColor.Black) => '♘',
            (PieceType.Pawn, PieceColor.Black) => '♟',
            _ => '?'
        };
    }

    public string GetName()
    {
        return Type.ToString();
    }

    public Piece Clone()
    {
        return new Piece(Type, Color) { HasMoved = HasMoved };
    }
}
