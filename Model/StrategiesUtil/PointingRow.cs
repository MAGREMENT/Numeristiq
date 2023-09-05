using System;
using System.Collections.Generic;
using System.Linq;
using Model.Positions;
using Model.StrategiesUtil.LinkGraph;

namespace Model.StrategiesUtil;

public class PointingRow : ILinkGraphElement
{
    public int Possibility { get; }
    public int Row { get; }
    private readonly LinePositions _pos;

    public int Count => _pos.Count;

    public PointingRow(int possibility, int row, LinePositions colPositions)
    {
        Possibility = possibility;
        Row = row;
        _pos = colPositions;
    }

    public PointingRow(int possibility, int row, params int[] cols)
    {
        Possibility = possibility;
        Row = row;
        _pos = new LinePositions();
        foreach(var col in cols)
        {
            _pos.Add(col);
        }
    }

    public PointingRow(int possibility, IEnumerable<Coordinate> coords)
    {
        Possibility = possibility;
        Row = coords.First().Row;
        _pos = new LinePositions();
        foreach (var coord in coords)
        {
            if (coord.Row != Row) throw new ArgumentException("Not on same row");
            _pos.Add(coord.Col);
        }
    }
    
    public PointingRow(int possibility, IEnumerable<PossibilityCoordinate> coords)
    {
        Possibility = possibility;
        Row = coords.First().Row;
        _pos = new LinePositions();
        foreach (var coord in coords)
        {
            if (coord.Row != Row) throw new ArgumentException("Not on same row");
            _pos.Add(coord.Col);
        }
    }

    public IEnumerable<PossibilityCoordinate> SharedSeenCells(PossibilityCoordinate single)
    {
        if (single.Row == Row)
        {
            for (int col = 0; col < 9; col++)
            {
                if (col == single.Col || _pos.Any(posCol=> posCol == col)) continue;
                yield return new PossibilityCoordinate(Row, col, Possibility);
            }
        }
        if (single.Row / 3 == Row / 3 && single.Col / 3 == _pos.First() / 3)
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
                        (row == Row && _pos.Any(posCol => posCol == col))) continue;
                    yield return new PossibilityCoordinate(row, col, Possibility);
                }
            }
        }
    }
    
    public IEnumerable<PossibilityCoordinate> SharedSeenCells(PointingRow row)
    {
        if(Row != row.Row) yield break;
        for (int col = 0; col < 9; col++)
        {
            if (row._pos.Any(posCol => posCol == col) ||
                _pos.Any(posCol => posCol == col)) continue;
            yield return new PossibilityCoordinate(Row, col, Possibility);
        }
    }

    public override int GetHashCode()
    {
        int coordsHash = 0;
        foreach (var col in _pos)
        {
            coordsHash ^= col.GetHashCode();
        }

        return HashCode.Combine(Possibility, Row, coordsHash);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointingRow pr) return false;
        if (pr.Possibility != Possibility || pr.Row != Row || _pos.Count != pr._pos.Count) return false;
        foreach (var col in _pos)
        {
            if (!pr._pos.Peek(col)) return false;
        }

        return true;
    }

    public override string ToString()
    {
        var result = "[PR : ";
        foreach (var col in _pos)
        {
            result += $"{Row + 1}, {col + 1} | ";
        }

        result = result[..^2];
        return result + $"=> {Possibility}]";
    }

    public CoordinatePossibilities[] EachElement()
    {
        CoordinatePossibilities[] result = new CoordinatePossibilities[_pos.Count];
        
        int cursor = 0;
        foreach (var col in _pos)
        {
            result[cursor] = new CoordinatePossibilities(new Coordinate(Row, col), Possibility);
            cursor++;
        }

        return result;
    }
}