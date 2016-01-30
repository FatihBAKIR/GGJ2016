public struct Coord
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public Coord(int x, int y) : this()
    {
        X = x;
        Y = y;
    }

    public static Coord operator +(Coord lhs, Coord rhs)
    {
        return new Coord(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    public override string ToString()
    {
        return string.Format("({0}, {1})", X, Y);
    }
}