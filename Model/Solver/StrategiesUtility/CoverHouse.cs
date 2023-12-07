using System;
using Global;

namespace Model.Solver.StrategiesUtility;

public readonly struct CoverHouse
{
    public CoverHouse(Unit unit, int number)
    {
        Unit = unit;
        Number = number;
    }

    public Unit Unit { get; }
    public int Number { get; }

    public override bool Equals(object? obj)
    {
        return obj is CoverHouse ch && ch == this;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Unit, Number);
    }

    public override string ToString()
    {
        var s = Unit switch
        {
            Unit.Row => "r",
            Unit.Column => "c",
            Unit.MiniGrid => "b"
        };

        return $"{s}{Number + 1}";
    }

    public static bool operator ==(CoverHouse left, CoverHouse right)
    {
        return left.Unit == right.Unit && left.Number == right.Number;
    }

    public static bool operator !=(CoverHouse left, CoverHouse right)
    {
        return !(left == right);
    }

    public bool Contains(Cell cell)
    {
        return Unit switch
        {
            Unit.Row => cell.Row == Number,
            Unit.Column => cell.Column == Number,
            Unit.MiniGrid => cell.Row / 3 * 3 + cell.Column == Number,
            _ => false
        };
    }

    public bool Overlaps(CoverHouse house)
    {
        switch (Unit, house.Unit)
        {
            case (Unit.Row, Unit.Column) :
            case (Unit.Column, Unit.Row) :
                return true;
            case (Unit.Row, Unit.MiniGrid) :
            case (Unit.MiniGrid, Unit.Row) :
                return Number / 3 == house.Number / 3;
            case (Unit.Column, Unit.MiniGrid) :
                return Number / 3 == house.Number % 3;
            case (Unit.MiniGrid, Unit.Column) :
                return Number % 3 == Number / 3;
            case (Unit.Row, Unit.Row) :
            case (Unit.Column, Unit.Column) :
            case (Unit.MiniGrid, Unit.MiniGrid) :
                return Number == house.Number;
        }

        return false;
    }
}

public record PossibilityCovers(int Possibility, CoverHouse[] CoverHouses);

public readonly struct PossibilityCoverHouse
{
    public int Possibility { get; }
    public CoverHouse CoverHouse { get; }
    
    public PossibilityCoverHouse(int possibility, CoverHouse coverHouse)
    {
        Possibility = possibility;
        CoverHouse = coverHouse;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, CoverHouse.GetHashCode());
    }

    public override bool Equals(object? obj)
    {
        return obj is PossibilityCoverHouse pch && pch == this;
    }

    public static bool operator ==(PossibilityCoverHouse left, PossibilityCoverHouse right)
    {
        return left.Possibility == right.Possibility && left.CoverHouse == right.CoverHouse;
    }

    public static bool operator !=(PossibilityCoverHouse left, PossibilityCoverHouse right)
    {
        return !(left == right);
    }
}