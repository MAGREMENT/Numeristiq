﻿using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility;

public readonly struct House
{
    public House(Unit unit, int number)
    {
        Unit = unit;
        Number = number;
    }

    public Unit Unit { get; }
    public int Number { get; }

    public override bool Equals(object? obj)
    {
        return obj is House ch && ch == this;
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
            Unit.Box => "b",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{s}{Number + 1}";
    }

    public static bool operator ==(House left, House right)
    {
        return left.Unit == right.Unit && left.Number == right.Number;
    }

    public static bool operator !=(House left, House right)
    {
        return !(left == right);
    }

    public Cell GetCellAt(int unit)
    {
        return Unit == Unit.Row
            ? new Cell(Number, unit)
            : new Cell(unit, Number);
    }

    public IEnumerable<Cell> EnumerateCells()
    {
        for (int i = 0; i < 9; i++)
        {
            yield return GetCellAt(i);
        }
    }

    public bool Contains(Cell cell)
    {
        return Unit switch
        {
            Unit.Row => cell.Row == Number,
            Unit.Column => cell.Column == Number,
            Unit.Box => cell.Row / 3 * 3 + cell.Column == Number,
            _ => false
        };
    }

    public bool Overlaps(House house)
    {
        switch (Unit, house.Unit)
        {
            case (Unit.Row, Unit.Column) :
            case (Unit.Column, Unit.Row) :
                return true;
            case (Unit.Row, Unit.Box) :
            case (Unit.Box, Unit.Row) :
                return Number / 3 == house.Number / 3;
            case (Unit.Column, Unit.Box) :
                return Number / 3 == house.Number % 3;
            case (Unit.Box, Unit.Column) :
                return Number % 3 == Number / 3;
            case (Unit.Row, Unit.Row) :
            case (Unit.Column, Unit.Column) :
            case (Unit.Box, Unit.Box) :
                return Number == house.Number;
        }

        return false;
    }

    public (Cell, Cell) GetExtremities()
    {
        switch (Unit)
        {
            case Unit.Row : 
                return (new Cell(Number, 0), new Cell(Number, 8));
            case Unit.Column :
                return (new Cell(0, Number), new Cell(8, Number));
            case Unit.Box :
                var sRow = Number / 3 * 3;
                var sCol = Number % 3 * 3;
                return (new Cell(sRow, sCol), new Cell(sRow + 2, sCol + 2));
            default:
                return (new Cell(), new Cell());
        }
    }
}

public record PossibilityCovers(int Possibility, House[] CoverHouses);