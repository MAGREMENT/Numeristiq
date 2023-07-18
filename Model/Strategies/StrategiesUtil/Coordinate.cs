using System;
using System.Collections.Generic;

namespace Model.Strategies.StrategiesUtil;

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

    public static bool ShareAUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 ||
               (row1 / 3 == row2 / 3
                && col1 / 3 == col2 / 3);
    }

    public IEnumerable<Coordinate> SharedSeenCells(Coordinate coord)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if ((row == Row && col == Col) || (row == coord.Row && col == coord.Col)) continue;
                
                if (ShareAUnit(row, col, Row, Col)
                    && ShareAUnit(row, col, coord.Row, coord.Col))
                {
                    yield return new Coordinate(row, col); 
                }
                
            }
        }
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

public class PossibilityCoordinate : Coordinate
{
    public int Possibility { get; }
    
    public PossibilityCoordinate(int row, int col, int possibility) : base(row, col)
    {
        Possibility = possibility;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PossibilityCoordinate pc) return false;
        return pc.Possibility == Possibility && pc.Row == Row && pc.Col == Col;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1} => {Possibility}]";
    }
}