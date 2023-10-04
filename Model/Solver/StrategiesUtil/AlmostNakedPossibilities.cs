using System;
using System.Linq;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil;

public class AlmostNakedPossibilities : ILinkGraphElement
{
    public CellPossibilities[] CoordinatePossibilities { get; }
    public CellPossibility OddOne { get; }

    public AlmostNakedPossibilities(CellPossibilities[] coordinates, CellPossibility oddOne)
    {
        CoordinatePossibilities = coordinates;
        OddOne = oddOne;
    }

    public bool Contains(int row, int col)
    {
        foreach (var coord in CoordinatePossibilities)
        {
            if (coord.Cell.Row == row && coord.Cell.Col == col) return true;
        }

        return false;
    }

    public CellPossibilities[] EachElement()
    {
        return CoordinatePossibilities;
    }

    public Cell[] EveryCell()
    {
        Cell[] result = new Cell[CoordinatePossibilities.Length];
        for (int i = 0; i < CoordinatePossibilities.Length; i++)
        {
            result[i] = CoordinatePossibilities[i].Cell;
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostNakedPossibilities anp) return false;
        if (anp.OddOne != OddOne) return false;
        foreach (CellPossibilities cp in CoordinatePossibilities)
        {
            if (!anp.CoordinatePossibilities.Contains(cp)) return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hashCode = 0;
        foreach (var coord in CoordinatePossibilities)
        {
            hashCode ^= coord.GetHashCode();
        }

        return HashCode.Combine(OddOne.GetHashCode(), hashCode);
    }

    public override string ToString()
    {
        var result = $"[ALS ({OddOne.Possibility}) : ";
        foreach (var coord in CoordinatePossibilities)
        {
            result += $"{coord} | ";
        }

        return result[..^2] + "]";
    }
}