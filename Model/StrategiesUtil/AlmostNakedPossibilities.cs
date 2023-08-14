using System;
using System.Linq;
using Model.Possibilities;

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


    public PossibilityCoordinate[] EachElement()
    {
        //TODO
        return Array.Empty<PossibilityCoordinate>();
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

public class CoordinatePossibilities
{
    public CoordinatePossibilities(Coordinate coordinate, IPossibilities possibilities)
    {
        Coordinate = coordinate;
        Possibilities = possibilities;
    }

    public Coordinate Coordinate { get; }
    public IPossibilities Possibilities { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not CoordinatePossibilities cp) return false;
        return Coordinate == cp.Coordinate && Possibilities.Equals(cp.Possibilities);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Coordinate.GetHashCode(), Possibilities.GetHashCode());
    }

    public override string ToString()
    {
        return $"{Coordinate} => {Possibilities}";
    }
}