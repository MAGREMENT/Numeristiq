using System;

namespace Model;

public class Coordinate
{
    public int Row { get; }
    public int Col { get; }


    public Coordinate(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Coordinate coord) return false;
        return Row == coord.Row && Col == coord.Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1}]";
    }
}