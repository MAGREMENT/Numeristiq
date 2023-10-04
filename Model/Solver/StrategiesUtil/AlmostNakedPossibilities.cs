using System;
using System.Linq;
using Model.Solver.Possibilities;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil;

public class AlmostNakedPossibilities : ILinkGraphElement
{
    public CellPossibilities[] CellPossibilities { get; }
    public CellPossibility OddOne { get; }

    public AlmostNakedPossibilities(CellPossibilities[] cells, CellPossibility oddOne)
    {
        CellPossibilities = cells;
        OddOne = oddOne;
    }

    public bool Contains(int row, int col)
    {
        foreach (var coord in CellPossibilities)
        {
            if (coord.Cell.Row == row && coord.Cell.Col == col) return true;
        }

        return false;
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        return CellPossibilities;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[CellPossibilities.Length];
        for (int i = 0; i < CellPossibilities.Length; i++)
        {
            result[i] = CellPossibilities[i].Cell;
        }

        return result;
    }

    public IPossibilities EveryPossibilities()
    {
        IPossibilities result = IPossibilities.NewEmpty();
        result.Add(OddOne.Possibility);

        foreach (var cp in CellPossibilities)
        {
            result = result.Or(cp.Possibilities);
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostNakedPossibilities anp) return false;
        if (anp.OddOne != OddOne) return false;
        foreach (CellPossibilities cp in CellPossibilities)
        {
            if (!anp.CellPossibilities.Contains(cp)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var coord in CellPossibilities)
        {
            hashCode ^= coord.GetHashCode();
        }

        return HashCode.Combine(OddOne.GetHashCode(), hashCode);
    }

    public override string ToString()
    {
        var result = $"ALS : {EveryPossibilities()}";
        foreach (var coord in CellPossibilities)
        {
            result += $"{coord.Cell}, ";
        }

        return result[..^2];
    }
}