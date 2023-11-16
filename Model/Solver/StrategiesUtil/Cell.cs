using System;
using System.Collections.Generic;
using Global;
using Model.Solver.Possibility;
using Model.Solver.StrategiesUtil.LinkGraph;
using Model.Solver.StrategiesUtil.SharedSeenCellSearchers;

namespace Model.Solver.StrategiesUtil;

public static class Cells
{
    private static readonly ISharedSeenCellSearcher Searcher = new InCommonFindSearcher();
    
    public static bool ShareAUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 || (row1 / 3 == row2 / 3 && col1 / 3 == col2 / 3);
    }

    public static bool ShareAUnit(Cell one, Cell two)
    {
        return ShareAUnit(one.Row, one.Col, two.Row, two.Col);
    }

    public static bool ShareAUnitWithAll(Cell cell, List<Cell> cells)
    {
        foreach (var c in cells)
        {
            if (!ShareAUnit(cell, c)) return false;
        }

        return true;
    }

    public static List<Cell> SeenCells(Cell cell)
    {
        List<Cell> result = new();
        
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.Row) result.Add(new Cell(i, cell.Col));
            if (i != cell.Col) result.Add(new Cell(cell.Row, i));
        }

        var startRow = cell.Row / 3 * 3;
        var startCol = cell.Col / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var row = startRow + i;
                var col = startCol + i;

                if (row != cell.Row && col != cell.Col) result.Add(new Cell(row, col));
            }
        }

        return result;
    }
    
    public static IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2) //TODO "AsList"
    {
        return Searcher.SharedSeenCells(row1, col1, row2, col2);
    }

    public static IEnumerable<Cell> SharedSeenCells(Cell one, Cell two)
    {
        return SharedSeenCells(one.Row, one.Col, two.Row, two.Col);
    }

    public static IEnumerable<Cell> SharedSeenCells(Cell one, Cell two, params Cell[] others)
    {
        foreach (var coord in SharedSeenCells(one, two))
        {
            bool ok = true;
            foreach (var other in others)
            {
                if (!ShareAUnit(other, coord) || other == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return coord;
        }
    }
    
    public static List<Cell> SharedSeenCells(List<Cell> list)
    {
        if (list.Count == 0) return new List<Cell>();
        if (list.Count == 1) return SeenCells(list[^1]);

        var result = new List<Cell>();
        foreach (var coord in SharedSeenCells(list[0], list[1]))
        {
            bool ok = true;
            for (int i = 2; i < list.Count; i++)
            {
                if (!ShareAUnit(list[i], coord) || list[i] == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) result.Add(coord);
        }

        return result;
    }

    public static IEnumerable<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, int row1, int col1, int row2, int col2)
    {
        return Searcher.SharedSeenEmptyCells(strategyManager, row1, col1, row2, col2);
    }
    
    public static List<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, List<Cell> list)
    {
        if (list.Count == 0) return new List<Cell>();
        if (list.Count == 1) return SeenCells(list[^1]); //TODO change to SeenEmptyCells

        var result = new List<Cell>();
        foreach (var coord in Searcher.SharedSeenEmptyCells(strategyManager, list[0].Row, list[0].Col,
                     list[1].Row, list[1].Col))
        {
            bool ok = true;
            for (int i = 2; i < list.Count; i++)
            {
                if (!ShareAUnit(list[i], coord) || list[i] == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) result.Add(coord);
        }

        return result;
    }

    public static List<Cell> SharedSeenEmptyCells(IStrategyManager strategyManager, Cell one, Cell two,
        params Cell[] others)
    {
        List<Cell> result = new List<Cell>();
        foreach (var coord in SharedSeenEmptyCells(strategyManager, one.Row, one.Col, two.Row, two.Col))
        {
            bool ok = true;
            foreach (var other in others)
            {
                if (!ShareAUnit(other, coord) || other == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) result.Add(coord);
        }

        return result;
    }
    
    public static bool AreSpreadOverTwoBoxes(int row1, int col1, int row2, int col2)
    {
        return (row1 / 3 != row2 / 3) ^ (col1 / 3 != col2 / 3);
    }
    
    public static IEnumerable<Cell[]> DeadlyPatternRoofs(Cell[] floor)
    {
        if (floor.Length != 2) yield break;
        
        if (floor[0].Row == floor[1].Row)
        {
            if (floor[0].Col / 3 == floor[1].Col / 3)
            {
                var miniRow = floor[0].Row / 3;
                for (int row = 0; row < 9; row++)
                {
                    if (miniRow == row / 3) continue;

                    yield return new[]
                    {
                        new Cell(row, floor[0].Col),
                        new Cell(row, floor[1].Col)
                    };
                }
            }
            else
            {
                var startRow = floor[0].Row / 3 * 3;
                for (int row = startRow; row < startRow + 3; row++)
                {
                    if (row == floor[0].Row) continue;

                    yield return new[]
                    {
                        new Cell(row, floor[0].Col),
                        new Cell(row, floor[1].Col)
                    };
                }
            }
        }
        else if (floor[0].Col == floor[1].Col)
        {
            if (floor[0].Row / 3 == floor[1].Row / 3)
            {
                var miniCol = floor[0].Col / 3;
                for (int col = 0; col < 9; col++)
                {
                    if (miniCol == col / 3) continue;

                    yield return new[]
                    {
                        new Cell(floor[0].Row, col),
                        new Cell(floor[1].Row, col)
                    };
                }
            }
            else
            {
                var startCol = floor[0].Col / 3 * 3;
                for (int col = startCol; col < startCol + 3; col++)
                {
                    if (col == floor[0].Col) continue;

                    yield return new[]
                    {
                        new Cell(floor[0].Row, col), 
                        new Cell(floor[1].Row, col)
                    };
                }
            }
        }
        else
        {
            if (!AreSpreadOverTwoBoxes(floor[0].Row, floor[0].Col, floor[1].Row, floor[1].Col)) yield break;
        
            yield return new[]
            {
                new Cell(floor[0].Row, floor[1].Col),
                new Cell(floor[1].Row, floor[0].Col)
            };  
        }
    }

    public static Dictionary<int[], int[]> DiagonalMiniGridAssociation(int miniRowExcept, int miniColExcept)
    {
        var result = new Dictionary<int[], int[]>(4);
        List<int[]> buffer = new(4);

        for (int miniRow = 0; miniRow < 3; miniRow++)
        {
            if (miniRow == miniRowExcept) continue;
            
            for (int miniCol = 0; miniCol < 3; miniCol++)
            {
                if (miniCol == miniColExcept) continue;
                
                buffer.Add(new[] {miniRow, miniCol});
            }
        }
        
        result.Add(buffer[0], buffer[3]);
        result.Add(buffer[1], buffer[2]);
        result.Add(buffer[2], buffer[1]);
        result.Add(buffer[3], buffer[0]);

        return result;
    }
}

public interface ICellPossibility
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; } 
}

public readonly struct CellPossibility : ILinkGraphElement, ICellPossibility
{
    public int Possibility { get; }
    public int Row { get; }
    public int Col { get; }

    public CellPossibility(int row, int col, int possibility)
    {
        Possibility = possibility;
        Row = row;
        Col = col;
    }

    public CellPossibility(Cell coord, int possibility)
    {
        Possibility = possibility;
        Row = coord.Row;
        Col = coord.Col;
    }
    
    public bool ShareAUnit(CellPossibility coord)
    {
        return Cells.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }
    
    public bool ShareAUnit(Cell coord)
    {
        return Cells.ShareAUnit(Row, Col, coord.Row, coord.Col);
    }

    public IEnumerable<Cell> SharedSeenCells(CellPossibility coord)
    {
        return Cells.SharedSeenCells(Row, Col, coord.Row, coord.Col);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Possibility, Row, Col);
    }

    public override bool Equals(object? obj)
    {
        return (obj is CellPossibility cp && cp == this) ||
               (obj is ICellPossibility icp && icp.Possibility == Possibility && icp.Row == Row && icp.Col == Col);
    }

    public override string ToString()
    {
        return $"{Possibility}[{Row + 1}, {Col + 1}]";
    }

    public CellPossibilities[] EveryCellPossibilities()
    {
        return new[] { new CellPossibilities(this) };
    }

    public Cell[] EveryCell()
    {
        return new Cell[] { new(Row, Col) };
    }

    public Possibilities EveryPossibilities()
    {
        var result = Possibilities.NewEmpty();
        result.Add(Possibility);
        return result;
    }

    public Cell ToCell()
    {
        return new Cell(Row, Col);
    }

    public static bool operator ==(CellPossibility left, CellPossibility right)
    {
        return left.Possibility == right.Possibility && left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(CellPossibility left, CellPossibility right)
    {
        return !(left == right);
    }
}

public class CellPossibilities
{
    public Cell Cell { get; }
    public IReadOnlyPossibilities Possibilities { get; }
    
    public CellPossibilities(Cell cell, IReadOnlyPossibilities possibilities)
    {
        Cell = cell;
        Possibilities = possibilities;
    }

    public CellPossibilities(Cell cell, int possibility)
    {
        Cell = cell;
        var buffer = Possibility.Possibilities.NewEmpty();
        buffer.Add(possibility);
        Possibilities = buffer;
    }
    
    public CellPossibilities(CellPossibility coord)
    {
        Cell = new Cell(coord.Row, coord.Col);
        var buffer = Possibility.Possibilities.NewEmpty();
        buffer.Add(coord.Possibility);
        Possibilities = buffer;
    }

    public CellPossibility[] ToCellPossibility()
    {
        var result = new CellPossibility[Possibilities.Count];

        var cursor = 0;
        foreach (var possibility in Possibilities)
        {
            result[cursor] = new CellPossibility(Cell.Row, Cell.Col, possibility);
            cursor++;
        }

        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CellPossibilities cp) return false;
        return Cell == cp.Cell && Possibilities.Equals(cp.Possibilities);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Cell.GetHashCode(), Possibilities.GetHashCode());
    }

    public override string ToString()
    {
        return $"{Cell} => {Possibilities}";
    }
}