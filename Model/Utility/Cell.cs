using System;

namespace Model.Utility;

public readonly struct Cell
{
    public int Row { get; }
    public int Column { get; }


    public Cell(int row, int col)
    {
        Row = row;
        Column = col;
    }
    
    public bool IsAdjacentTo(Cell other)
    {
        var rowDiff = Math.Abs(Row - other.Row);
        var colDiff = Math.Abs(Column - other.Column);
        
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Column);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Cell coord) return false;
        return Row == coord.Row && Column == coord.Column;
    }

    public override string ToString()
    {
        return $"r{Row + 1}c{Column + 1}";
    }

    public static bool operator ==(Cell left, Cell right)
    {
        return left.Row == right.Row && left.Column == right.Column;
    }

    public static bool operator !=(Cell left, Cell right)
    {
        return !(left == right);
    }
}