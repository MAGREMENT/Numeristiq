using System;
using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;
using Model.StrategiesUtil.LinkGraph;

namespace Model.StrategiesUtil;

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
        var result = $"[ANP ({OddOne.Possibility}) : ";
        foreach (var coord in CoordinatePossibilities)
        {
            result += $"{coord} | ";
        }

        return result[..^2] + "]";
    }
}