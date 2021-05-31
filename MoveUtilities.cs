using ChessParser;
using System;
using System.Collections.Generic;
using System.Linq;

static class MoveUtilities
{
    public static List<Coordinate> GetCandidateBishopMoves(ChessBoard chessBoard, int rank, int file,
        PieceColor turn)
    {
        List<Coordinate> moves = GetCandidateStraightOrDiagonalMoves(chessBoard, -1, -1, rank, file, turn);
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, 1, 1, rank, file, turn));
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, -1, 1, rank, file, turn));
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, 1, -1, rank, file, turn));
        return moves;
    }

    public static List<Coordinate> GetCandidateRookMoves(ChessBoard chessBoard, int rank, int file, 
        PieceColor turn)
    {
        List<Coordinate> moves = GetCandidateStraightOrDiagonalMoves(chessBoard, 1, 0, rank, file, turn);
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, -1, 0, rank, file, turn));
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, 0, 1, rank, file, turn));
        moves.AddRange(GetCandidateStraightOrDiagonalMoves(chessBoard, 0, -1, rank, file, turn));
        return moves;
    }

    public static List<Coordinate> GetCandidateQueenMoves(ChessBoard chessBoard, int rank, int file,
        PieceColor turn)
    {
        List<Coordinate> moves = GetCandidateBishopMoves(chessBoard, rank, file, turn);
        moves.AddRange(GetCandidateRookMoves(chessBoard, rank, file, turn));
        return moves;
    }


    // Consider two options below.
    // Option one: move these kind of methods to ChessBoard class.
    // Option two: Create a new BoardMoveValidator class, and feed it to ChessBoard class.
    public static List<Coordinate> GetCandidateStraightOrDiagonalMoves(ChessBoard chessBoard, int dx, int dy, 
        int rank, int file, PieceColor stoppingColor)
    {
        List<Coordinate> moves = new List<Coordinate>();
        int x = rank;
        int y = file;

        while (LocationInBounds(x + dx, y + dy))
        {            
            x += dx;
            y += dy;

            moves.Add(new Coordinate(x, y));

            Piece piece = chessBoard.PieceAt(x, y);
            if (piece.PieceColor == stoppingColor)
            {
                break;
            }
        }

        return moves;
    }

    public static List<Coordinate> GetCandidateKnightMoves(int rank, int file)
    {
        int[] dirs = new int[] { -2, 1, -2, -1, 1, -2, 1, 2, -1, -2, -1, 2, 2, -1, 2, 1 };
        return GetCandidateMovesFromDirections(dirs, rank, file);
    }

    public static List<Coordinate> GetCandidateKingMoves(int rank, int file)
    {
        int[] dirs = { -1, -1, -1, 0, -1, 1, 0, -1, 0, 1, 1, -1, 1, 0, 1, 1 };
        return GetCandidateMovesFromDirections(dirs, rank, file);
    }

    public static List<Coordinate> GetCandidateMovesFromDirections(int[] dirs, int rank, int file)
    {
        List<Coordinate> candidateMoves = new List<Coordinate>();
        for (int i = 0; i < dirs.Length - 1; i+=2)
        {
            if (LocationInBounds(rank + dirs[i], file + dirs[i + 1]))
            {
                candidateMoves.Add(new Coordinate(rank + dirs[i], file + dirs[i + 1]));
            }
        }

        return candidateMoves;
    }

    // Fundamental Issue: This is written assuming that we know the piece to move's location, and want to look
    // at its possible moves, but we want to instead look at moves that have a piece that we are looking for BEFORE
    // the piece is actually moved. We also don't have the piece's location beforehand, as that is what we are
    // looking for.
    public static List<Coordinate> FilterCandidateMoves(ChessBoard chessBoard, List<Coordinate> candidateMoves,
        int rank, int file, PieceColor turn, bool pieceIsKnight)
    {
        List<Coordinate> moves = new List<Coordinate>();
        IEnumerable<Coordinate> sortedCandidateMoves = 
            candidateMoves.OrderBy(c => GetDistance(c.X, rank)).ThenBy(c => GetDistance(c.Y, file));
        foreach (Coordinate move in sortedCandidateMoves)
        {
            Piece pieceAtLocation = chessBoard.PieceAt(move.X, move.Y);
            if (pieceAtLocation.PieceType == PieceType.Empty)
            {
                moves.Add(move);
            } else if (pieceAtLocation.PieceColor != turn)
            {
                moves.Add(move);
                break;
            }
            else if (!pieceIsKnight)
            {
                break;
            }
        }

        return moves;
    }

    private static int GetDistance(int x1, int x2)
    {
        return Math.Abs(x1 - x2);
    }

    private static List<Coordinate> GetDiagonalOrStraightAttacks(ChessBoard chessBoard, int c1, int c2, int r3, 
        int r4, int rank, int file, PieceColor turn)
    {
        List<Coordinate> line1 = GetCandidateStraightOrDiagonalMoves(chessBoard, 1, c1, rank, file, turn);
        List<Coordinate> line2 = GetCandidateStraightOrDiagonalMoves(chessBoard, -1, c2, rank, file, turn);
        List<Coordinate> line3 = GetCandidateStraightOrDiagonalMoves(chessBoard, r3, 1, rank, file, turn);
        List<Coordinate> line4 = GetCandidateStraightOrDiagonalMoves(chessBoard, r4, -1, rank, file, turn);

        List<Coordinate> filtered1 = FilterCandidateMoves(chessBoard, line1, rank, file, turn, false);
        List<Coordinate> filtered2 = FilterCandidateMoves(chessBoard, line2, rank, file, turn, false);
        List<Coordinate> filtered3 = FilterCandidateMoves(chessBoard, line3, rank, file, turn, false);
        List<Coordinate> filtered4 = FilterCandidateMoves(chessBoard, line4, rank, file, turn, false);

        List<Coordinate> moves = new List<Coordinate>();
        moves.AddRange(filtered1);
        moves.AddRange(filtered2);
        moves.AddRange(filtered3);
        moves.AddRange(filtered4);

        return moves;
    } 

    public static List<Coordinate> GetDiagonalAttacks(ChessBoard chessBoard, Coordinate location, PieceColor turn)
    {
        return GetDiagonalOrStraightAttacks(chessBoard, 1, -1, -1, 1, location.X, location.Y, turn);
    }

    public static List<Coordinate> GetStraightAttacks(ChessBoard chessBoard, Coordinate location, PieceColor turn)
    {
        return GetDiagonalOrStraightAttacks(chessBoard, 0, 0, 0, 0, location.X, location.Y, turn);
    }

    public static List<Coordinate> GetKnightAttacks(ChessBoard chessBoard, Coordinate location, PieceColor turn)
    {
        List<Coordinate> attacks = GetCandidateKnightMoves(location.X, location.Y);
        return FilterCandidateMoves(chessBoard, attacks, location.X, location.Y, turn, true);
    }

    public static List<Coordinate> GetKingAttacks(ChessBoard chessBoard, Coordinate location)
    {
        List<Coordinate> attacks = GetCandidateKingMoves(location.X, location.Y);
        PieceColor turn = chessBoard.PieceAt(location.X, location.Y).PieceColor;
        return FilterCandidateMoves(chessBoard, attacks, location.X, location.Y, turn, false);
    }

    public static bool LocationInBounds(int rank, int file)
    {
        return rank >= 0 && rank < 8 && file >= 0 && file < 8;
    }

    public static bool PieceIsEnemy(PieceColor player, Piece enemy)
    {
        return player != enemy.PieceColor;
    }
}
