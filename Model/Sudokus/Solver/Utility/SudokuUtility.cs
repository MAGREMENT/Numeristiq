using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Core.Changes;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Utility;

public static class SudokuUtility
{
    public static CellPossibility[] Cast(IReadOnlyList<NumericChange> changes)
    {
        var result = new CellPossibility[changes.Count];
        for (int i = 0; i < changes.Count; i++)
        {
            var c = changes[i];
            result[i] = new CellPossibility(c.Row, c.Column, c.Number);
        }

        return result;
    }
    
    public static int SharedRow(IEnumerable<Cell> cells)
    {
        int r = -1;

        foreach (var cell in cells)
        {
            if (r != -2)
            {
                if (r == -1) r = cell.Row;
                else if (r != cell.Row) r = -2;
            }
        }

        return r;
    }

    public static int SharedColumn(IEnumerable<Cell> cells)
    {
        int c = -1;

        foreach (var cell in cells)
        {
            if (c != -2)
            {
                if (c == -1) c = cell.Column;
                else if (c != cell.Column) c = -2;
            }
        }

        return c;
    }

    public static (int, int) SharedLines(IEnumerable<Cell> cells)
    {
        int r = -1;
        int c = -1;
        
        foreach (var cell in cells)
        {
            if (r != -2)
            {
                if (r == -1) r = cell.Row;
                else if (r != cell.Row) r = -2;
            }
            
            if (c != -2)
            {
                if (c == -1) c = cell.Column;
                else if (c != cell.Column) c = -2;
            }
        }

        return (r, c);
    }
    
    public static bool ShareAUnit(int row1, int col1, int row2, int col2)
    {
        return row1 == row2 || col1 == col2 || (row1 / 3 == row2 / 3 && col1 / 3 == col2 / 3);
    }

    public static bool ShareAUnit(Cell one, Cell two)
    {
        return ShareAUnit(one.Row, one.Column, two.Row, two.Column);
    }

    public static bool ShareAUnitWithAll(Cell cell, List<Cell> cells)
    {
        foreach (var c in cells)
        {
            if (!ShareAUnit(cell, c)) return false;
        }

        return true;
    }

    public static IEnumerable<Cell> SeenCells(Cell cell)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.Row) yield return new Cell(i, cell.Column);
            if (i != cell.Column) yield return new Cell(cell.Row, i);
        }

        var startRow = cell.Row / 3 * 3;
        var startCol = cell.Column / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var row = startRow + i;
                var col = startCol + i;

                if (row != cell.Row && col != cell.Column) yield return new Cell(row, col);
            }
        }
    }
    
    public static IEnumerable<Cell> SharedSeenCells(int row1, int col1, int row2, int col2)
    {
        return SharedSeenAlgorithms.InCommonSharedSeenCells(row1, col1, row2, col2);
    }

    public static IEnumerable<Cell> SharedSeenCells(Cell one, Cell two)
    {
        return SharedSeenCells(one.Row, one.Column, two.Row, two.Column);
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
    
    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyList<Cell> list)
    {
        return list.Count switch
        {
            0 => new List<Cell>(),
            1 => SeenCells(list[^1]),
            _ => CheckedSeenCells(list)
        };
    }
    
    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyList<CellPossibilities> list)
    {
        return list.Count switch
        {
            0 => new List<Cell>(),
            1 => SeenCells(list[^1].Cell),
            _ => CheckedSeenCells(list)
        };
    }

    private static IEnumerable<Cell> CheckedSeenCells(IReadOnlyList<Cell> list)
    {
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

            if (ok) yield return coord;
        }
    }
    
    private static IEnumerable<Cell> CheckedSeenCells(IReadOnlyList<CellPossibilities> list)
    {
        foreach (var coord in SharedSeenCells(list[0].Cell, list[1].Cell))
        {
            bool ok = true;
            for (int i = 2; i < list.Count; i++)
            {
                if (!ShareAUnit(list[i].Cell, coord) || list[i].Cell == coord)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return coord;
        }
    }
    
    public static IEnumerable<Cell> SeenEmptyCells(ISudokuSolvingState holder, Cell cell)
    {
        for (int i = 0; i < 9; i++)
        {
            if (i != cell.Row && holder[i, cell.Column] == 0) yield return new Cell(i, cell.Column);
            if (i != cell.Column && holder[cell.Row, i] == 0) yield return new Cell(cell.Row, i);
        }

        var startRow = cell.Row / 3 * 3;
        var startCol = cell.Column / 3 * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var row = startRow + i;
                var col = startCol + i;

                if (row != cell.Row && col != cell.Column && holder[row, col] == 0) yield return new Cell(row, col);
            }
        }
    }

    public static IEnumerable<Cell> SharedSeenEmptyCells(ISudokuSolverData solverData, int row1, int col1, int row2, int col2)
    {
        return SharedSeenAlgorithms.InCommonSharedSeenEmptyCells(solverData, row1, col1, row2, col2);
    }
    
    public static IEnumerable<Cell> SharedSeenEmptyCells(ISudokuSolverData solverData, IReadOnlyList<Cell> list)
    {
        return list.Count switch
        {
            0 => new List<Cell>(),
            1 => SeenEmptyCells(solverData, list[^1]),
            _ => CheckedSharedSeenEmptyCells(solverData, list)
        };
    }

    private static IEnumerable<Cell> CheckedSharedSeenEmptyCells(ISudokuSolverData solverData, IReadOnlyList<Cell> list)
    {
        foreach (var coord in SharedSeenEmptyCells(solverData, list[0].Row, list[0].Column,
                     list[1].Row, list[1].Column))
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

            if (ok) yield return coord;
        }
    }

    public static IEnumerable<Cell> SharedSeenEmptyCells(ISudokuSolverData solverData, Cell one, Cell two,
        params Cell[] others)
    {
        foreach (var coord in SharedSeenEmptyCells(solverData, one.Row, one.Column, two.Row, two.Column))
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
    
    public static bool AreSpreadOverTwoBoxes(int row1, int col1, int row2, int col2)
    {
        return (row1 / 3 != row2 / 3) ^ (col1 / 3 != col2 / 3);
    }

    public static IEnumerable<CellPossibility> SeenExistingPossibilities(ISudokuSolverData solverData, CellPossibility cp)
    {
        foreach (var poss in solverData.PossibilitiesAt(cp.Row, cp.Column).EnumeratePossibilities())
        {
            if (poss == cp.Possibility) continue;
            
            yield return new CellPossibility(cp.Row, cp.Column, poss);
        }

        foreach (var col in solverData.RowPositionsAt(cp.Row, cp.Possibility))
        {
            if (col == cp.Column) continue;

            yield return new CellPossibility(cp.Row, col, cp.Possibility);
        }
        
        foreach (var row in solverData.ColumnPositionsAt(cp.Column, cp.Possibility))
        {
            if (row == cp.Row) continue;

            yield return new CellPossibility(row, cp.Column, cp.Possibility);
        }
        
        foreach (var pos in solverData.MiniGridPositionsAt(cp.Row / 3, cp.Column / 3, cp.Possibility))
        {
            if (pos == cp.ToCell()) continue;

            yield return new CellPossibility(pos, cp.Possibility);
        }
    }

    public static IEnumerable<CellPossibility> SharedSeenExistingPossibilities(ISudokuSolverData solverData, CellPossibility first,
        CellPossibility second)
    {
        return SharedSeenAlgorithms.InCommonSharedSeenExistingPossibilities(solverData, first.Row, 
            first.Column, first.Possibility,
            second.Row, second.Column, second.Possibility);
    }

    public static IEnumerable<CellPossibility> SharedSeenExistingPossibilities(ISudokuSolverData solverData,
        IReadOnlyList<CellPossibility> list)
    {
        return list.Count switch
        {
            0 => Enumerable.Empty<CellPossibility>(),
            1 => SeenExistingPossibilities(solverData, list[0]),
            _ => CheckedSharedSeenExistingPossibilities(solverData, list)
        };
    }

    private static IEnumerable<CellPossibility> CheckedSharedSeenExistingPossibilities(ISudokuSolverData solverData,
        IReadOnlyList<CellPossibility> list)
    {
        foreach (var cp in SharedSeenExistingPossibilities(solverData, list[0], list[1]))
        {
            bool ok = true;
            for (int i = 2; i < list.Count; i++)
            {
                if (!AreLinked(cp, list[i]) || list[i] == cp)
                {
                    ok = false;
                    break;
                }
            }

            if(ok) yield return cp;
        }
    }
    
    public static bool AreLinked(CellPossibility first, CellPossibility second)
    {
        if (first == second) return false;
        return (first.Row == second.Row && first.Column == second.Column) ||
               (ShareAUnit(first.Row, first.Column, second.Row, second.Column) && first.Possibility == second.Possibility);
    }

    public static IEnumerable<CellPossibility> GetStrongLinks(ISudokuSolverData solverData, CellPossibility cp)
    {
        var poss = solverData.PossibilitiesAt(cp.Row, cp.Column);
        if (poss.Count == 2) yield return new CellPossibility(cp.Row, cp.Column, poss.FirstPossibility(cp.Possibility));

        var rPos = solverData.RowPositionsAt(cp.Row, cp.Possibility);
        if (rPos.Count == 2) yield return new CellPossibility(cp.Row, rPos.First(cp.Column), cp.Possibility);

        var cPos = solverData.ColumnPositionsAt(cp.Column, cp.Possibility);
        if (cPos.Count == 2) yield return new CellPossibility(cPos.First(cp.Row), cp.Column, cp.Possibility);

        var mPos = solverData.MiniGridPositionsAt(cp.Row / 3, cp.Column / 3, cp.Possibility);
        if (mPos.Count == 2) yield return new CellPossibility(mPos.First(cp.ToCell()), cp.Possibility);
    }

    public static bool AreStronglyLinked(ISudokuSolverData solverData, CellPossibility cp1, CellPossibility cp2)
    {
        if (cp1.Row == cp2.Row && cp1.Column == cp2.Column)
            return solverData.PossibilitiesAt(cp1.Row, cp2.Column).Count == 2;

        if (cp1.Possibility == cp2.Possibility)
        {
            return (cp1.Row == cp2.Row && solverData.RowPositionsAt(cp1.Row, cp1.Possibility).Count == 2) ||
                   (cp1.Column == cp2.Column &&
                    solverData.ColumnPositionsAt(cp1.Column, cp1.Possibility).Count == 2) ||
                   (cp1.Row / 3 == cp2.Row / 3 && cp1.Column / 3 == cp2.Column / 3 && solverData
                       .MiniGridPositionsAt(cp1.Row / 3, cp1.Column / 3, cp1.Possibility).Count == 2);
        }

        return false;
    }
    
    public static IEnumerable<Cell[]> DeadlyPatternRoofs(Cell[] floor)
    {
        if (floor.Length != 2) yield break;
        
        if (floor[0].Row == floor[1].Row)
        {
            if (floor[0].Column / 3 == floor[1].Column / 3)
            {
                var miniRow = floor[0].Row / 3;
                for (int row = 0; row < 9; row++)
                {
                    if (miniRow == row / 3) continue;

                    yield return new[]
                    {
                        new Cell(row, floor[0].Column),
                        new Cell(row, floor[1].Column)
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
                        new Cell(row, floor[0].Column),
                        new Cell(row, floor[1].Column)
                    };
                }
            }
        }
        else if (floor[0].Column == floor[1].Column)
        {
            if (floor[0].Row / 3 == floor[1].Row / 3)
            {
                var miniCol = floor[0].Column / 3;
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
                var startCol = floor[0].Column / 3 * 3;
                for (int col = startCol; col < startCol + 3; col++)
                {
                    if (col == floor[0].Column) continue;

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
            if (!AreSpreadOverTwoBoxes(floor[0].Row, floor[0].Column, floor[1].Row, floor[1].Column)) yield break;
        
            yield return new[]
            {
                new Cell(floor[0].Row, floor[1].Column),
                new Cell(floor[1].Row, floor[0].Column)
            };  
        }
    }

    public static IEnumerable<(MiniGrid, MiniGrid)> DiagonalMiniGridAssociation(int miniRowExcept, int miniColExcept)
    {
        int r1, r2;
        int c1, c2;

        switch (miniRowExcept)
        {
            case 0 : r1 = 1;
                r2 = 2;
                break;
            case 1 : r1 = 0;
                r2 = 2;
                break;
            case 2 : r1 = 0;
                r2 = 1;
                break;
            default: yield break;
        }
        
        switch (miniColExcept)
        {
            case 0 : c1 = 1;
                c2 = 2;
                break;
            case 1 : c1 = 0;
                c2 = 2;
                break;
            case 2 : c1 = 0;
                c2 = 1;
                break;
            default: yield break;
        }

        yield return (new MiniGrid(r1, c1), new MiniGrid(r2, c2));
        yield return (new MiniGrid(r1, c2), new MiniGrid(r2, c1));
    }
}

