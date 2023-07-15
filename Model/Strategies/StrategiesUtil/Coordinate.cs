using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Model.Strategies.ChainingStrategiesUtil;

public class Coordinate
{
    public int Row { get; }
    public int Col { get; }


    public Coordinate(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public bool ShareAUnit(Coordinate coord)
    {
        return Row == coord.Row || Col == coord.Col ||
               (Row / 3 == coord.Row / 3
                && Col / 3 == coord.Col / 3);
    }

    public IEnumerable<Coordinate> SharedSeenCells(Coordinate coord)
    {
        HashSet<Coordinate> result = new();
        //Same MiniGrid
        if (Row / 3 == coord.Row / 3 && Col / 3 == coord.Col / 3)
        {
            int rowStart = Row / 3 * 3;
            int colStart = Col / 3 * 3;

            for (int miniRow = 0; miniRow < 3; miniRow++)
            {
                for (int miniCol = 0; miniCol < 3; miniCol++)
                {
                    result.Add(new Coordinate(rowStart + miniRow, colStart + miniCol));
                }
            }
        }
        //Same Row
        if (Row == coord.Row)
        {
            for (int col = 0; col < 9; col++)
            {
                result.Add(new Coordinate(Row, col));
            }
        }
        //Same Col
        if (Col == coord.Col)
        {
            for (int row = 0; row < 9; row++)
            {
                result.Add(new Coordinate(row, Col));
            }
        }

        result.Add(new Coordinate(Row, coord.Col));
        result.Add(new Coordinate(coord.Row, Col));
        result.Remove(new Coordinate(Row, Col));
        result.Remove(new Coordinate(coord.Row, coord.Col));

        return result;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Coordinate coord) return false;
        return Row == coord.Row && Col == coord.Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1}]";
    }
}

public class ColoringCoordinate : Coordinate, IColorable
{
    public Coloring Coloring { get; set; } = Coloring.None;

    public ColoringCoordinate(int row, int col) : base(row, col)
    {
    }
}

public interface IColorable
{
    public Coloring Coloring { get; set; }
}