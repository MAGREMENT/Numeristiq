using System;

namespace Global;

public readonly struct Cell
{
    public int Row { get; }
    public int Col { get; }


    public Cell(int row, int col)
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
        if (obj is not Cell coord) return false;
        return Row == coord.Row && Col == coord.Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1}]";
    }

    public static bool operator ==(Cell left, Cell right)
    {
        return left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }
}