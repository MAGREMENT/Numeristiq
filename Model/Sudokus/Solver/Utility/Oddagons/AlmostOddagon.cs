using System;
using System.Linq;
using Model.Sudokus.Solver.Utility.Graphs;
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
        return HashCode.Combine(Loop.GetHashCode(), Guardians.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostOddagon ao || !ao.Loop.Equals(Loop) || ao.Guardians.Length != Guardians.Length)
            return false;

        foreach (var g in Guardians)
        {
            if (!ao.Guardians.Contains(g)) return false;
        }

        return true;
    }
}