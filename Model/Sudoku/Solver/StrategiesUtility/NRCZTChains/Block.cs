using System;

namespace Model.Sudoku.Solver.StrategiesUtility.NRCZTChains;

public readonly struct Block
{
    public Block(CellPossibility start, CellPossibility end)
    {
        Start = start;
        End = end;
    }

    public CellPossibility Start { get; }
    public CellPossibility End { get; }

    public override bool Equals(object? obj)
    {
        return obj is Block b && this == b;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(Block left, Block right)
    {
        return left.Start == right.Start && left.End == right.End;
    }

    public static bool operator !=(Block left, Block right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        if (Start.Possibility == End.Possibility)
        {
            return $"n{Start.Possibility}" + " {" + $"r{Start.Row + 1}c{Start.Column + 1} " +
                   $"r{End.Row + 1}c{End.Column + 1}" + "}";
        }

        if (Start.Row == End.Row && Start.Column == End.Column)
        {
            return $"n{Start.Possibility} n{End.Possibility}" + " {" + $"r{Start.Row + 1}c{Start.Column + 1}" + "}";
        }

        return "?";
    }
}