using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace ChessParser
{
    class GameParser
    {
        public string[,] Moves { get; }
        public ChessBoard Board { get; }

        public GameParser(string fileName)
        {
            ChessBoard chessBoard = new ChessBoard();
            Board = chessBoard;
            ReadFileMoves(fileName);
        }

        private void ReadFileMoves(string fileName)
        {
            foreach (var line in File.ReadLines(fileName))
            {
                string[] plies = line.Split(' ', 4);
                ExecuteMove(plies[1], PieceColor.White);
                ExecuteMove(plies[2], PieceColor.Black);
                Console.WriteLine(Board);
            }
        }

        public void ExecuteMove(string move, PieceColor turn)
        {
            if (move == "O-O")
            {
                Board.CastleShort(turn);
            }
            else if (move == "O-O-O")
            {
                Board.CastleLong(turn);
            }
            else if (move.Length == 2)
            {
                Coordinate end = ParseEndSquare(move[0], move[1]);
                MovePawn(turn, end.X, end.Y);
            }
            else if (move.EndsWith('+'))
            {
                ExecuteMove(move[0..^1], turn);
            }
            else if (move.Length == 3)
            {
                Coordinate end = ParseEndSquare(move[1], move[2]);
                MovePiece(move[0], turn, end, false);
            }
            else if (move.Length == 4 && move[1] == 'x')
            {
                Coordinate end = ParseEndSquare(move[2], move[3]);
                if (char.IsLower(move[0]))
                {
                    CaptureWithPawn(turn, end.X, end.Y, move[0]);
                }
                else
                {
                    MovePiece(move[0], turn, end, true);
                }
            }
            else if (move.Length == 4)
            {
                Coordinate end = ParseEndSquare(move[2], move[3]);
                if (char.IsLetter(move[1]))
                {
                    MoveUniquePiece(move[0], turn, false, move[1] - 'a', end);
                }
                else if (char.IsDigit(move[1]))
                {
                    MoveUniquePiece(move[0], turn, true, 8 - (move[1] - '0'), end);
                }
            }
        }

        private void MoveUniquePiece(char piece, PieceColor turn, bool onRank,
        int captureSquare, Coordinate end)
        {
            Piece piece1 = null;

            if (piece == 'R')
            {
                piece1 = new Rook(turn);
            }
            else if (piece == 'N')
            {
                piece1 = new Knight(turn);
            }
            else if (piece == 'B')
            {
                piece1 = new Bishop(turn);
            }
            else if (piece == 'Q')
            {
                piece1 = new Queen(turn);
            }

            if (onRank)
            {
                int file = FindPieceFileGivenRank(piece1, captureSquare);
                Piece piece2 = Board.PieceAt(captureSquare, file);
                Board.MovePiece(captureSquare, file, end.X, end.Y, piece2);
            }
            else
            {
                int rank = FindPieceRankGivenFile(piece1, captureSquare);
                Piece piece2 = Board.PieceAt(rank, captureSquare);
                Board.MovePiece(rank, captureSquare, end.X, end.Y, piece2);
            }
        }

        private void MovePawn(PieceColor turn, int rank, int file)
        {
            if (PawnMovedTwoSquares(turn, rank, file))
            {
                MovePawnTwoSquares(turn, rank, file);
            }
            else if (PawnMovedOneSquare(turn, rank, file))
            {
                MovePawnOneSquare(turn, rank, file);
            }
        }

        private bool PawnMovedTwoSquares(PieceColor turn, int rank, int file)
        {
            bool whitePawnMoved = rank == 4 && turn == PieceColor.White;
            bool blackPawnMoved = rank == 3 && turn == PieceColor.Black;

            if (!whitePawnMoved && !blackPawnMoved)
            {
                return false;
            }

            int rankOffset = (turn == PieceColor.White) ? 1 : -1;

            if (!MoveUtilities.LocationInBounds(rank + rankOffset, file)
            || !MoveUtilities.LocationInBounds(rank + 2 * rankOffset, file))
            {
                return false;
            }

            Piece oneSquareBack = Board.PieceAt(rank + rankOffset, file);
            Piece twoSquaresBack = Board.PieceAt(rank + 2 * rankOffset, file);

            bool sameTurn = twoSquaresBack.PieceColor == turn;
            bool isPawn = twoSquaresBack.PieceType == PieceType.Pawn;
            bool isEmpty = oneSquareBack.PieceType == PieceType.Empty;

            return sameTurn && isPawn && isEmpty;
        }

        private bool PawnMovedOneSquare(PieceColor turn, int rank, int file)
        {
            bool whitePawnMoved = rank == 4 && turn == PieceColor.White;
            bool blackPawnMoved = rank == 3 && turn == PieceColor.Black;

            bool pawnMoved = whitePawnMoved || blackPawnMoved;

            int rankOffset = (turn == PieceColor.White) ? 1 : -1;
            Piece piece = Board.PieceAt(rank, file);

            if (!MoveUtilities.LocationInBounds(rank + rankOffset, file))
            {
                return false;
            }

            Piece oneSquareBack = Board.PieceAt(rank + rankOffset, file);

            bool sameTurn = oneSquareBack.PieceColor == turn;
            bool isPawn = oneSquareBack.PieceType == PieceType.Pawn;
            bool isEmpty = piece.PieceType == PieceType.Empty;

            return pawnMoved || (sameTurn && isPawn && isEmpty);
        }

        private void MovePawnOneSquare(PieceColor turn, int rank, int file)
        {
            int rankOffset = (turn == PieceColor.White) ? 1 : -1;
            int[] moves = new int[] { rank + rankOffset, file };
            MovePawnHelper(moves, rank, file, turn, rankOffset);
        }

        private void MovePawnTwoSquares(PieceColor turn, int rank, int file)
        {
            int rankOffset = (turn == PieceColor.White) ? 2 : -2;
            int[] moves = new int[] { rank + rankOffset, file };
            MovePawnHelper(moves, rank, file, turn, rankOffset);
        }

        private void MovePawnHelper(int[] moves, int rank, int file, PieceColor turn, int rankOffset)
        {
            Piece piece = Board.PieceAt(rank + rankOffset, file);
            if (!Board.MoveWouldCauseCheck(rank + rankOffset, file, rank, file, piece, turn))
            {
                MovePieceAtLocation(moves, rank, file);
            }
        }

        private void CaptureWithPawn(PieceColor turn, int rank, int file, char captureFile)
        {
            int captureFileInt = captureFile - 'a';
            int rankOffset = (turn == PieceColor.White) ? 1 : -1;
            int[] pawnLocation = new int[] { rank + rankOffset, captureFileInt };
            EnPassantWithPawn(turn, rank, file, pawnLocation[0], pawnLocation[1]);
            MovePieceAtLocation(pawnLocation, rank, file);
        }

        private void EnPassantWithPawn(PieceColor turn, int rank, int file, int captureRank, int captureFile)
        {
            int fileOffset = (turn == PieceColor.White) ? -1 : 1;
            Piece piece = Board.PieceAt(rank, file);
            if (piece.PieceType == PieceType.Empty)
            {
                Board.RemovePiece(captureRank, captureFile + fileOffset);
            }
        }

        private static Coordinate ParseEndSquare(char rank, char file)
        {
            return new Coordinate(8 - (file - '0'), rank - 'a');
        }

        private int[] FindOriginalPiece(List<Coordinate> moves, Coordinate end, PieceColor player, PieceType type, 
            bool isCapture)
        {
            foreach (Coordinate move in moves)
            {
                if (MoveUtilities.LocationInBounds(move.X, move.Y))
                {
                    Piece piece = Board.PieceAt(move.X, move.Y);
                    if (piece.PieceColor == player && piece.PieceType == type
                        && !Board.MoveWouldCauseCheck(move.X, move.Y, end.X, end.Y, piece, player))
                    {
                        return new int[] { move.X, move.Y };
                    }
                }
            }

            return null;
        }

        private void MovePieceAtLocation(int[] location, int rank, int file)
        {
            if (location != null)
            {
                Piece piece = Board.PieceAt(location[0], location[1]);
                Board.MovePiece(location[0], location[1], rank, file, piece);
            }
        }

        private void MovePiece(char piece, PieceColor turn, Coordinate end, bool isCapture)
        {
            List<Coordinate> candidateMoves = new List<Coordinate>();

            if (piece == 'R')
            {
                candidateMoves = MoveUtilities.GetCandidateRookMoves(Board, end.X, end.Y, turn);
            }
            else if (piece == 'N')
            {
                candidateMoves = MoveUtilities.GetCandidateKnightMoves(end.X, end.Y);
            }
            else if (piece == 'B')
            {
                candidateMoves = MoveUtilities.GetCandidateBishopMoves(Board, end.X, end.Y, turn);
            }
            else if (piece == 'Q')
            {
                candidateMoves = MoveUtilities.GetCandidateQueenMoves(Board, end.X, end.Y, turn);
            }
            else if (piece == 'K')
            {
                candidateMoves = MoveUtilities.GetCandidateKingMoves(end.X, end.Y);
            }

            if (candidateMoves.Count() > 0)
            {
                PieceType type = GetPieceTypeFromLetter(piece);
                int[] location = FindOriginalPiece(candidateMoves, end, turn, type, isCapture);
                MovePieceAtLocation(location, end.X, end.Y);
            }
        }

        private static PieceType GetPieceTypeFromLetter(char piece)
        {
            return piece switch
            {
                'N' => PieceType.Knight,
                'B' => PieceType.Bishop,
                'R' => PieceType.Rook,
                'Q' => PieceType.Queen,
                'K' => PieceType.King,
                _ => PieceType.Pawn,
            };
        }

        private int FindPieceRankGivenFile(Piece piece, int file)
        {
            for (int rank = 0; rank < 8; rank++)
            {
                if (Board.PieceAt(rank, file).Equals(piece))
                {
                    return rank;
                }
            }

            return -1;
        }

        private int FindPieceFileGivenRank(Piece piece, int rank)
        {
            for (int file = 0; file < 8; file++)
            {
                if (Board.PieceAt(rank, file).Equals(piece))
                {
                    return file;
                }
            }

            return -1;
        }

        public void PrintMoves()
        {
            for (int i = 0; i < Moves.GetLength(0); i++)
            {
                for (int j = 0; j < Moves.GetLength(1); j++)
                {
                    Console.WriteLine(Moves[i, j]);
                }
            }
        }
    }
}
