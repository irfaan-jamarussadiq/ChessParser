using System;
using System.Collections.Generic;

namespace ChessParser
{
    public class ChessBoard
    {
        private readonly Piece[,] board;
        public Coordinate BlackKing { set; get; }
        public Coordinate WhiteKing { set; get; }

        public ChessBoard()
        {
            board = new Piece[8, 8];
            SetUpStartingPosition();
            BlackKing = new Coordinate(0, 4);
            WhiteKing = new Coordinate(7, 4);
        }

        public ChessBoard(ChessBoard boardCopy)
        {
            this.board = new Piece[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    this.board[i, j] = boardCopy.PieceAt(i, j);
                }
            }

            this.WhiteKing = boardCopy.WhiteKing;
            this.BlackKing = boardCopy.BlackKing;
        }

        public Piece PieceAt(int row, int col)
        {
            return board[row, col];
        }

        private void SetUpStartingPosition()
        {
            SetUpPieceRow(0, PieceColor.Black);
            SetUpPawns(1, PieceColor.Black);

            for (int i = 2; i < 6; i++)
            {
                SetUpEmptyRow(i);
            }

            SetUpPawns(6, PieceColor.White);
            SetUpPieceRow(7, PieceColor.White);
        }

        private void SetUpPawns(int row, PieceColor pieceColor)
        {
            for (int i = 0; i < 8; i++)
            {
                AddPiece(row, i, new Pawn(pieceColor));
            }
        }

        private void SetUpPieceRow(int row, PieceColor pieceColor)
        {
            AddPiece(row, 0, new Rook(pieceColor));
            AddPiece(row, 1, new Knight(pieceColor));
            AddPiece(row, 2, new Bishop(pieceColor));
            AddPiece(row, 3, new Queen(pieceColor));
            AddPiece(row, 4, new King(pieceColor));
            AddPiece(row, 5, new Bishop(pieceColor));
            AddPiece(row, 6, new Knight(pieceColor));
            AddPiece(row, 7, new Rook(pieceColor));
        }

        private void SetUpEmptyRow(int row)
        {
            for (int i = 0; i < 8; i++)
            {
                board[row, i] = new Empty();
            }
        }

        /*
            Adding a piece means removing the piece from its current location,
            then adding the piece in the new location.
        */
        public void MovePiece(int startRow, int startCol, int endRow,
        int endCol, Piece piece)
        {
            RemovePiece(startRow, startCol);
            AddPiece(endRow, endCol, piece);

            UpdateKingPositions(endRow, endCol, piece);
        }

        private void UpdateKingPositions(int row, int col, Piece piece)
        {
            if (piece.PieceType == PieceType.King)
            {
                if (piece.PieceColor == PieceColor.Black)
                {
                    BlackKing = new Coordinate(row, col);
                }
                else if (piece.PieceColor == PieceColor.White)
                {
                    WhiteKing = new Coordinate(row, col);
                }
            }
        }

        public void CastleShort(PieceColor color)
        {
            int startRow = (color == PieceColor.Black) ? 0 : 7;
            MovePiece(startRow, 4, startRow, 6, PieceAt(startRow, 4));
            MovePiece(startRow, 7, startRow, 5, PieceAt(startRow, 7));
        }

        public void CastleLong(PieceColor color)
        {
            int startRow = (color == PieceColor.Black) ? 0 : 7;
            MovePiece(startRow, 4, startRow, 2, PieceAt(startRow, 4));
            MovePiece(startRow, 7, startRow, 3, PieceAt(startRow, 0));
        }

        public void RemovePiece(int row, int col)
        {
            board[row, col] = new Empty();
        }

        public void AddPiece(int row, int col, Piece piece)
        {
            board[row, col] = piece;
        }

        public bool MoveWouldCauseCheck(int startRow, int startCol, int endRow, int endCol, Piece piece,
            PieceColor player)
        {
            ChessBoard copy = new ChessBoard(this);
            copy.MovePiece(startRow, startCol, endRow, endCol, piece);

            Coordinate kingLocation = (player == PieceColor.Black) ? copy.BlackKing : copy.WhiteKing;
            return copy.KingIsInCheck(kingLocation);
        }

        public bool KingIsInCheck(Coordinate kingLocation)
        {
            PieceColor kingPlayer = kingLocation.Equals(BlackKing) ? PieceColor.Black : PieceColor.White;
            return KingIsAttackedOnKnightPath(kingLocation, kingPlayer)
                || KingIsAttackedOnDiagonalPath(kingLocation)
                || KingIsAttackedOnStraightPath(kingLocation);
        }

        private bool KingIsAttackedOnPath(List<Coordinate> attackLocations, PieceColor player, PieceType[] types)
        {
            foreach (Coordinate coordinate in attackLocations)
            {
                Piece piece = PieceAt(coordinate.X, coordinate.Y);
                if (piece.PieceColor != player && Array.Exists(types, t => t == piece.PieceType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool KingIsAttackedOnDiagonalPath(Coordinate kingLocation)
        {
            PieceColor turn = PieceAt(kingLocation.X, kingLocation.Y).PieceColor;
            List<Coordinate> attackLocations = MoveUtilities.GetDiagonalAttacks(this, kingLocation, turn);
            PieceType[] types = new PieceType[] { PieceType.Bishop, PieceType.Queen };
            return KingIsAttackedOnPath(attackLocations, turn, types);
        }

        private bool KingIsAttackedOnStraightPath(Coordinate kingLocation)
        {
            PieceColor turn = PieceAt(kingLocation.X, kingLocation.Y).PieceColor;
            List<Coordinate> attackLocations = MoveUtilities.GetStraightAttacks(this, kingLocation, turn);
            PieceType[] types = new PieceType[] { PieceType.Rook, PieceType.Queen };
            return KingIsAttackedOnPath(attackLocations, turn, types);
        }

        private bool KingIsAttackedOnKnightPath(Coordinate kingLocation, PieceColor player)
        {
            List<Coordinate> attackLocations = MoveUtilities.GetKnightAttacks(this, kingLocation, player);
            PieceType[] types = new PieceType[] { PieceType.Knight };
            return KingIsAttackedOnPath(attackLocations, player, types);
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Piece piece = PieceAt(i, j);
                    char letter = piece.PieceType.ToString()[0];

                    if (piece.PieceType == PieceType.Knight)
                    {
                        letter = 'N';
                    }

                    if (piece.PieceColor == PieceColor.Black)
                    {
                        letter = char.ToLower(letter);
                    }
                    else if (piece.PieceType == PieceType.Empty)
                    {
                        letter = '.';
                    }

                    result += letter.ToString() + ' ';
                }

                result += '\n';
            }

            return result;
        }
    }
}
