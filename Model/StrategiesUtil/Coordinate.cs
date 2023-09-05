using System;
using System.Collections.Generic;
using Model.Possibilities;
using Model.Solver;
using Model.StrategiesUtil.LinkGraph;
using Model.StrategiesUtil.LoopFinder;

namespace Model.StrategiesUtil;

public static class CoordinateUtils
{
    public static bool ShareAUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 ||
               (row1 / 3 == row2 / 3
                && col1 / 3 == col2 / 3);
    }
    
    public static IEnumerable<Coordinate> SharedSeenCells(int row1, int col1, int row2, int col2) //TODO : refactor (optimize)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if ((row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (ShareAUnit(row, col, row1, col1)
                    && ShareAUnit(row, col, row2, col2))
                {
                    yield return new Coordinate(row, col); 
                }
                
            }
        }
    }
    
    public static IEnumerable<Coordinate> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] != 0 ||
                    (row == row1 && col == col1) || (row == row2 && col == col2)) continue;
                
                if (ShareAUnit(row, col, row1, col1)
                    && ShareAUnit(row, col, row2, col2))
                {
                    yield return new Coordinate(row, col);  
                }
            }
        }
    }
}

public readonly struct Coordinate
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
        return CoordinateUtils.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }

    public IEnumerable<Coordinate> SharedSeenCells(Coordinate coord)
    {
        return CoordinateUtils.SharedSeenCells(Row, Col, coord.Row, coord.Col);
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

    public static bool operator ==(Coordinate left, Coordinate right)
    {
        return left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(Coordinate left, Coordinate right)
    {
        return !(left == right);
    }
}

public class CoordinateColoring : IColorable
{
    public Coordinate Coordinate { get; }
    public Coloring Coloring { get; set; } = Coloring.None;

    public CoordinateColoring(int row, int col)
    {
        Coordinate = new Coordinate(row, col);
    }

    public override int GetHashCode()
    {
        return Coordinate.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is CoordinateColoring cc && cc.Coordinate.Equals(Coordinate);
    }
}

public readonly struct PossibilityCoordinate : ILinkGraphElement
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; }

    public PossibilityCoordinate(int row, int col, int possibility)
    {
        Possibility = possibility;
        Row = row;
        Col = col;
    }
    
    public bool ShareAUnit(PossibilityCoordinate coord)
    {
        return CoordinateUtils.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }
    
    public bool ShareAUnit(Coordinate coord)
    {
        return CoordinateUtils.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }

    public IEnumerable<Coordinate> SharedSeenCells(PossibilityCoordinate coord)
    {
        return CoordinateUtils.SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }
    
    public IEnumerable<Coordinate> SharedSeenCells(Coordinate coord)
    {
        return CoordinateUtils.SharedSeenCells(Row, Col, coord.Row, coord.Col);
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

    public CoordinatePossibilities[] EachElement()
    {
        return new[] { new CoordinatePossibilities(this) };
    }

    public bool IsSameLoopElement(ILoopElement other)
    {
        return other is PossibilityCoordinate pc && pc.Equals(this);
    }

    public static bool operator ==(PossibilityCoordinate left, PossibilityCoordinate right)
    {
        return left.Possibility == right.Possibility && left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(PossibilityCoordinate left, PossibilityCoordinate right)
    {
        return !(left == right);
    }
}

public class PossibilityCoordinateColoring : IColorable
{
    public PossibilityCoordinate PossibilityCoordinate { get; }
    public Coloring Coloring { get; set; }
    
    public PossibilityCoordinateColoring(int row, int col, int possibility)
    {
        PossibilityCoordinate = new PossibilityCoordinate(row, col, possibility);
    }

    public override int GetHashCode()
    {
        return PossibilityCoordinate.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is PossibilityCoordinateColoring pcc && pcc.PossibilityCoordinate.Equals(PossibilityCoordinate);
    }
}

public class CoordinatePossibilities
{
    public Coordinate Coordinate { get; }
    public IPossibilities Possibilities { get; }
    
    public CoordinatePossibilities(Coordinate coordinate, IPossibilities possibilities)
    {
        Coordinate = coordinate;
        Possibilities = possibilities;
    }

    public CoordinatePossibilities(Coordinate coordinate, int possibility)
    {
        Coordinate = coordinate;
        Possibilities = IPossibilities.NewEmpty();
        Possibilities.Add(possibility);
    }
    
    public CoordinatePossibilities(PossibilityCoordinate coord)
    {
        Coordinate = new Coordinate(coord.Row, coord.Col);
        Possibilities = IPossibilities.NewEmpty();
        Possibilities.Add(coord.Possibility);
    }

    public PossibilityCoordinate[] ToPossibilityCoordinates()
    {
        var result = new PossibilityCoordinate[Possibilities.Count];

        var cursor = 0;
        foreach (var possibility in Possibilities)
        {
            result[cursor] = new PossibilityCoordinate(Coordinate.Row, Coordinate.Col, possibility);
            cursor++;
        }

        return result;
    }

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