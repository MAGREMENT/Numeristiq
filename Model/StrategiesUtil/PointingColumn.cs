﻿using System;
using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.StrategiesUtil.LoopFinder;

namespace Model.StrategiesUtil;

public class PointingColumn : ILinkGraphElement
{
    public int Possibility { get; }
    public int Column { get; }
    private readonly LinePositions _pos;

    public int Count => _pos.Count;

    public PointingColumn(int possibility, int column, LinePositions rowPos)
    {
        Possibility = possibility;
        Column = column;
        _pos = rowPos;
    }

    public PointingColumn(int possibility, int column, params int[] rows)
    {
        Possibility = possibility;
        Column = column;
        _pos = new LinePositions();
        foreach (var row in rows)
        {
            _pos.Add(row);
        }
    }

    public PointingColumn(int possibility, IEnumerable<Coordinate> coords)
    {
        Possibility = possibility;
        Column = coords.First().Col;
        _pos = new LinePositions();
        foreach (var coord in coords)
        {
            if (coord.Col != Column) throw new ArgumentException("Not on same column");
            _pos.Add(coord.Row);
        }
    }

    public IEnumerable<PossibilityCoordinate> SharedSeenCells(PossibilityCoordinate single)
    {
        if (single.Col == Column)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row == single.Row || _pos.Any(posRow => posRow == row)) continue;
                yield return new PossibilityCoordinate(row, single.Col, Possibility);
            }
        }
        if (single.Row / 3 == _pos.First() / 3 && single.Col / 3 == Column / 3)
        {
            int rowStart = single.Row / 3 * 3;
            int colStart = single.Col / 3 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int row = rowStart + gridRow;
                    int col = colStart + gridCol;

                    if ((row == single.Row && col == single.Col) ||
                        (_pos.Any(posRow => posRow == row) && col == Column)) continue;
                    yield return new PossibilityCoordinate(row, col, Possibility);
                }
            }
        }
    }
    
    public IEnumerable<PossibilityCoordinate> SharedSeenCells(PointingColumn col)
    {
        if(col.Column != Column) yield break;
        for (int row = 0; row < 9; row++)
        {
            if (col._pos.Any(posRow => posRow == row) ||
                _pos.Any(posRow => posRow == row)) continue;
            yield return new PossibilityCoordinate(row, Column, Possibility);
        }
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var row in _pos)
        {
            coordsHash ^= row;
        }

        return HashCode.Combine(Possibility, Column, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingColumn pc) return false;
        if (pc.Possibility != Possibility || pc.Column != Column || _pos.Count != pc._pos.Count) return false;
        foreach (var posRow in _pos)
        {
            if (!pc._pos.Peek(posRow)) return false;
        }

        return true;
    }
    
    public override string ToString()
    {
        var result = "[PC : ";
        foreach (var row in _pos)
        {
            result += $"{row + 1}, {Column + 1} | ";
        }

        result = result[..^2];
        return result + $"=> {Possibility}]";
    }
    
    public bool IsSameLoopElement(ILoopElement other)
    {
        return false; //TODO
    }

    public Coordinate[] EachCoordinates()
    {
        return new Coordinate[1]; //TODO
    }
}