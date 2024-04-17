using System;

namespace Model.Sudokus.Solver.StrategiesUtility.NRCZTChains;

public readonly struct ConjugateRelation
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
        return obj is ConjugateRelation cr && cr.From == From && cr.To == To;
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
}