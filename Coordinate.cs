public struct Coordinate
{
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Coordinate))
            return false;

        Coordinate coordinate = (Coordinate) obj;
        return coordinate.X == X && coordinate.Y == Y;
    }

    public override int GetHashCode()
    {
        return (X << 2) ^ Y;
    }
}
