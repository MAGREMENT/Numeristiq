using System;
using System.Linq;
using Model.Core.Graphs;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver.Utility.Oddagons;

public class AlmostOddagon
{
    public Loop<CellPossibility, LinkStrength> Loop { get; }
    public CellPossibility[] Guardians { get; }
    
    public AlmostOddagon(Loop<CellPossibility, LinkStrength> loop, CellPossibility[] guardians)
    {
        Loop = loop;
        Guardians = guardians;
    }

    public override string ToString()
    {
        return $"{Loop} with guardians {Guardians.ToStringSequence(", ")}";
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var e in Loop.Elements)
        {
            hash ^= e.GetHashCode();
        }

        var hash2 = 0;
        foreach (var g in Guardians)
        {
            hash ^= g.GetHashCode();
        }
        return HashCode.Combine(hash, hash2);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostOddagon ao || ao.Loop.Elements.Count != Loop.Elements.Count || ao.Guardians.Length != Guardians.Length)
            return false;

        foreach (var g in Guardians)
        {
            if (!ao.Guardians.Contains(g)) return false;
        }

        foreach (var e in Loop.Elements)
        {
            if (!ao.Loop.Contains(e)) return false;
        }

        return true;
    }
}