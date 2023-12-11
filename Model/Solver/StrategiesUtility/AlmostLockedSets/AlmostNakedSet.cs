using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.AlmostLockedSets;

public class AlmostNakedSet : ILinkGraphElement //TODO look into almost hidden set
{
    public CellPossibilities[] NakedSet { get; }
    public CellPossibility OddOne { get; }

    public AlmostNakedSet(CellPossibilities[] cells, CellPossibility oddOne)
    {
        NakedSet = cells;
        OddOne = oddOne;
    }

    public int Rank => 3;

    public CellPossibilities[] EveryCellPossibilities()
    {
        return NakedSet;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[NakedSet.Length];
        for (int i = 0; i < NakedSet.Length; i++)
        {
            result[i] = NakedSet[i].Cell;
        }

        return result;
    }

    public Possibilities EveryPossibilities()
    {
        Possibilities result = Possibilities.NewEmpty();

        foreach (var cp in NakedSet)
        {
            result = result.Or(cp.Possibilities);
        }

        return result;
    }

    public CellPossibility[] EveryCellPossibility()
    {
        List<CellPossibility> result = new();
        foreach (var cp in NakedSet)
        {
            foreach (var p in cp.Possibilities)
            {
                result.Add(new CellPossibility(cp.Cell, p));
            }
        }

        return result.ToArray();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostNakedSet anp) return false;
        if (anp.OddOne != OddOne) return false;
        foreach (CellPossibilities cp in NakedSet)
        {
            if (!anp.NakedSet.Contains(cp)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var coord in NakedSet)
        {
            hashCode ^= coord.GetHashCode();
        }

        return HashCode.Combine(OddOne.GetHashCode(), hashCode);
    }

    public override string ToString()
    {
        var result = $"ALS : {EveryPossibilities()}";
        foreach (var coord in NakedSet)
        {
            result += $"{coord.Cell}, ";
        }

        return result[..^2];
    }
}