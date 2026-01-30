namespace Chess.Core;

public class Position
{
    public int Row { get; set; }
    public int Col { get; set; }

    public Position(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public Position(string algebraic)
    {
        if (string.IsNullOrEmpty(algebraic) || algebraic.Length != 2)
            throw new ArgumentException("Invalid algebraic notation");

        Col = algebraic[0] - 'a';
        Row = algebraic[1] - '1';

        if (Row < 0 || Row > 7 || Col < 0 || Col > 7)
            throw new ArgumentException("Position out of bounds");
    }

    public string ToAlgebraic()
    {
        return $"{(char)('a' + Col)}{Row + 1}";
    }

    public bool IsValid()
    {
        return Row >= 0 && Row < 8 && Col >= 0 && Col < 8;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Position other)
            return Row == other.Row && Col == other.Col;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public override string ToString()
    {
        return ToAlgebraic();
    }
}
