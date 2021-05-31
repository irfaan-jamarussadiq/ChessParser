using System;
public abstract class Piece
{
    public PieceType PieceType { get; set; }
    public PieceColor PieceColor { get; set; }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Piece p = (Piece)obj;
            return p.PieceColor == PieceColor && p.PieceType == PieceType;
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PieceType, PieceColor);
    }
}

public class Empty : Piece
{
    public Empty()
    {
        this.PieceColor = PieceColor.Undefined;
        this.PieceType = PieceType.Empty;
    }
}

public class Pawn : Piece
{
    public Pawn(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.Pawn;
    }
}

public class Knight : Piece
{
    public Knight(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.Knight;
    }
}

public class Bishop : Piece
{
    public Bishop(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.Bishop;
    }
}

public class Rook : Piece
{
    public Rook(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.Rook;
    }
}

public class Queen : Piece
{
    public Queen(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.Queen;
    }
}

public class King : Piece
{
    public King(PieceColor pieceColor)
    {
        this.PieceColor = pieceColor;
        this.PieceType = PieceType.King;
    }
}
