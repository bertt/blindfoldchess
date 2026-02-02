extern alias GeraChess;
using Chess.Core;

namespace Chess.Console;

/// <summary>
/// Helper class for displaying chess board using Gera.Chess library
/// </summary>
public static class BoardDisplayHelper
{
    /// <summary>
    /// Converts our Board to a beautiful ASCII representation using Gera.Chess
    /// </summary>
    public static string GetAsciiBoard(Chess.Core.Board board)
    {
        try
        {
            // Convert our board to FEN
            string fen = board.ToFen();
            
            // Load into Gera.Chess board for display using extern alias
            var geraBoard = GeraChess::Chess.ChessBoard.LoadFromFen(fen);
            
            // Get their beautiful ASCII art
            return geraBoard.ToAscii();
        }
        catch (Exception ex)
        {
            // Fallback to FEN if something goes wrong
            return $"Error displaying board: {ex.Message}\nFEN: {board.ToFen()}";
        }
    }
}
