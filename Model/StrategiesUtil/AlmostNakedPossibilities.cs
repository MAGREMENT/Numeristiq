using System;
using System.Collections.Generic;
using System.Linq;
using Model.Possibilities;
using Model.StrategiesUtil.LinkGraph;

namespace Model.StrategiesUtil;

public class AlmostNakedPossibilities : ILinkGraphElement
{
    public CoordinatePossibilities[] CoordinatePossibilities { get; }
    public PossibilityCoordinate OddOne { get; }

    public AlmostNakedPossibilities(CoordinatePossibilities[] coordinates, PossibilityCoordinate oddOne)
    {
        CoordinatePossibilities = coordinates;
        OddOne = oddOne;
    }

    public bool Contains(int row, int col)
    {
        foreach (var coord in CoordinatePossibilities)
        {
            if (coord.Coordinate.Row == row && coord.Coordinate.Col == col) return true;
        }

        return false;
    }

    public CoordinatePossibilities[] EachElement()
    {
        return CoordinatePossibilities;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AlmostNakedPossibilities anp) return false;
        if (anp.OddOne != OddOne) return false;
        foreach (CoordinatePossibilities cp in CoordinatePossibilities)
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