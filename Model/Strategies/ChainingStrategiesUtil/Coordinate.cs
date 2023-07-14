using System;

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