using Chess;

namespace Chess.Console;

/// <summary>
/// Helper class for displaying chess board using Gera.Chess library
/// </summary>
public static class BoardDisplayHelper
{
    /// <summary>
    /// Converts our Board to a beautiful ASCII representation using Gera.Chess
    /// Colors are optimized for colorblind accessibility
    /// </summary>
    public static string GetAsciiBoard(ChessBoard board)
    {
        try
        {
            // Gera.Chess board already has ToAscii method
            string ascii = board.ToAscii();
            return ApplyColors(ascii);
        }
        catch (Exception ex)
        {
            // Fallback to FEN if something goes wrong
            return $"Error displaying board: {ex.Message}\nFEN: {board.ToFen()}";
        }
    }

    /// <summary>
    /// Applies ANSI colors optimized for colorblind accessibility
    /// White pieces: Bright White (clear, sharp)
    /// Black pieces: Bright Magenta (excellent visibility for all colorblind types)
    /// </summary>
    private static string ApplyColors(string ascii)
    {
        const string BRIGHT_WHITE = "\x1b[97m";   // White pieces (P R N B Q K)
        const string BRIGHT_MAGENTA = "\x1b[95m"; // Black pieces (p r n b q k) - colorblind friendly
        const string RESET = "\x1b[0m";

        var result = new System.Text.StringBuilder(ascii.Length * 2);
        
        foreach (char c in ascii)
        {
            if (char.IsUpper(c) && "PRNBQK".Contains(c))
            {
                // White pieces - Bright White
                result.Append(BRIGHT_WHITE).Append(c).Append(RESET);
            }
            else if (char.IsLower(c) && "prnbqk".Contains(c))
            {
                // Black pieces - Bright Magenta (paars/roze - goed voor kleurenblinden)
                result.Append(BRIGHT_MAGENTA).Append(c).Append(RESET);
            }
            else
            {
                // Board characters, dots, etc - no color
                result.Append(c);
            }
        }
        
        return result.ToString();
    }
}
