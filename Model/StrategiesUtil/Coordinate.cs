using System;
using System.Collections.Generic;
using Model.StrategiesUtil.LoopFinder;

namespace Model.StrategiesUtil;

public class Coordinate //Reorganise this
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
        return SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }

    public IEnumerable<Coordinate> SharedSeenCellsV2(Coordinate coord)
    {
        if (Row == coord.Row)
        {
            for (int col = 0; col < 9; col++)
            {
                if (col == Col || col == coord.Col) continue;
                yield return new Coordinate(Row, col);
            }

            if (Col / 3 == coord.Col / 3)
            {
                int rowStart = Row / 3 * 3;
                int colStart = Col / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    int row = rowStart + gridRow;
                    if (row == Row) continue;
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        yield return new Coordinate(row, colStart + gridCol);
                    }
                }
            }
            
            yield break;
        }
        if (Col == coord.Col)
        {
            for (int row = 0; row < 9; row++)
            {
                if (row == Row || row == coord.Row) continue;
                yield return new Coordinate(row, Col);
            }
            
            if (Row / 3 == coord.Row / 3)
            {
                int rowStart = Row / 3 * 3;
                int colStart = Col / 3 * 3;

                for (int gridCol = 0; gridCol < 3; gridCol++)
                {
                    int col = colStart + gridCol;
                    if (col == Row) continue;
                    for (int gridRow = 0; gridRow < 3; gridRow++)
                    {
                        yield return new Coordinate(rowStart + gridRow, col);
                    }
                }
            }
            
            yield break;
        }
        if (Row / 3 == coord.Row / 3)
        {
            if (Col / 3 == coord.Col / 3)
            {
                int rowStart = Row / 3 * 3;
                int colStart = Col / 3 * 3;

                for (int gridRow = 0; gridRow < 3; gridRow++)
                {
                    for (int gridCol = 0; gridCol < 3; gridCol++)
                    {
                        int row = rowStart + gridRow;
                        int col = colStart + gridCol;
                        if ((row == Row && col == Col) || (row == coord.Row && col == coord.Col)) continue;
                        yield return new Coordinate(row, col);
                    }
                }
                yield break;
            }
            
            int colStart1 = Col / 3 * 3;
            int colStart2 = coord.Col / 3 * 3;

            for (int gridCol = 0; gridCol < 3; gridCol++)
            {
                int col1 = colStart1 + gridCol;
                int col2 = colStart2 + gridCol;

                yield return new Coordinate(coord.Row, col1);
                yield return new Coordinate(Row, col2);
            }
            
            yield break;
        }

        if (Col / 3 == coord.Col / 3)
        {
            int rowStart1 = Row / 3 * 3;
            int rowStart2 = coord.Row / 3 * 3;

            for (int gridRow = 0; gridRow < 3; gridRow++)
            {
                int row1 = rowStart1 + gridRow;
                int row2 = rowStart2 + gridRow;

                yield return new Coordinate(row1, coord.Col);
                yield return new Coordinate(row2, Col);
            }
            
            yield break;
        }

        yield return new Coordinate(Row, coord.Col);
        yield return new Coordinate(coord.Row, Col);
    }

    public static IEnumerable<Coordinate> SharedSeenCells(int row1, int col1, int row2, int col2)
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

public class PossibilityCoordinate : Coordinate, ILoopElement, ILinkGraphElement
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

    public bool IsSameLoopElement(ILoopElement other)
    {
        return other is PossibilityCoordinate pc && pc.Equals(this);
    }
}

public readonly struct CondensedPossibilityCoordinate
{
    private readonly int _n;

    public int Possibility => _n >> 8;
    public int Row => (_n >> 4) & 0xF;
    public int Column => _n & 0xF;

    public CondensedPossibilityCoordinate(int row, int col, int possibility)
    {
        _n = possibility << 8 | row << 4 | col;
    }

    public override int GetHashCode()
    {
        return _n;
    }

    public override bool Equals(object? obj)
    {
        return obj is CondensedPossibilityCoordinate cpc && cpc._n == _n;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Column + 1}] => {Possibility}";
    }

    public static bool operator ==(CondensedPossibilityCoordinate left, CondensedPossibilityCoordinate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CondensedPossibilityCoordinate left, CondensedPossibilityCoordinate right)
    {
        return !(left == right);
    }
}

public class MedusaCoordinate : ColoringCoordinate
{
    public int Number { get; }
    
    public MedusaCoordinate(int row, int col, int number) : base(row, col)
    {
        Number = number;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col, Number);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MedusaCoordinate coord) return false;
        return coord.Row == Row && coord.Col == Col && coord.Number == Number;
    }

    public override string ToString()
    {
        return $"[{Row + 1}, {Col + 1} => {Number}]";
    }
}