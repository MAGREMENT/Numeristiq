using System;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.NRCZTChains;

public readonly struct ConjugateRelation : IEquatable<ConjugateRelation>
{
    public CellPossibility From { get; }
    public CellPossibility To { get; }
    
    public ConjugateRelation(CellPossibility from, CellPossibility to)
    {
        From = from;
        To = to;
    }

    public override bool Equals(object? obj)
    {
        return obj is ConjugateRelation cr && Equals(cr);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }
    
    public override string ToString()
    {
        if (From.Possibility == To.Possibility)
        {
            return $"n{From.Possibility}" + " {" + $"r{From.Row + 1}c{From.Column + 1} " +
                   $"r{To.Row + 1}c{To.Column + 1}" + "}";
        }

        if (From.Row == To.Row && From.Column == To.Column)
        {
            return $"n{From.Possibility} n{To.Possibility}" + " {" + $"r{From.Row + 1}c{From.Column + 1}" + "}";
        }

        return "?";
    }
    public static bool operator ==(ConjugateRelation left, ConjugateRelation right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ConjugateRelation left, ConjugateRelation right)
    {
        return !(left == right);
    }

    public bool Equals(ConjugateRelation other)
    {
        return From.Equals(other.From) && To.Equals(other.To);
    }
}