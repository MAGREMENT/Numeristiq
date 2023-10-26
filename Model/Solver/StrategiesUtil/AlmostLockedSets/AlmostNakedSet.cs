using System;
using System.Linq;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.AlmostLockedSets;

public class AlmostNakedSet : ILinkGraphElement //TODO look into almost hidden set
{
    public CellPossibilities[] NakedSet { get; }
    public CellPossibility OddOne { get; }

    public AlmostNakedSet(CellPossibilities[] cells, CellPossibility oddOne)
    {
        NakedSet = cells;
        OddOne = oddOne;
    }

    public bool Contains(int row, int col)
    {
        foreach (var coord in NakedSet)
        {
            if (coord.Cell.Row == row && coord.Cell.Col == col) return true;
        }

        return false;
    }

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

    public IPossibilities EveryPossibilities()
    {
        IPossibilities result = IPossibilities.NewEmpty();

        foreach (var cp in NakedSet)
        {
            result = result.Or(cp.Possibilities);
        }

        return result;
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