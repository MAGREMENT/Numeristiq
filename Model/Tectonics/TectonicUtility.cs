using System;
using System.Collections.Generic;
using System.Linq;
using Model.Tectonics.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics;

public static class TectonicUtility
{
    public static bool AreNeighbors(Cell c1, Cell c2)
        => c1 != c2 && Math.Abs(c1.Row - c2.Row) <= 1 && Math.Abs(c1.Column - c2.Column) <= 1;
    
    public static bool AreNeighborsOrSame(Cell c1, Cell c2)
        => Math.Abs(c1.Row - c2.Row) <= 1 && Math.Abs(c1.Column - c2.Column) <= 1;
    
    public static bool DoesSeeEachOther(IReadOnlyTectonic tectonic, Cell one, Cell two)
    {
        return AreNeighbors(one, two) || tectonic.GetZone(one).Contains(two);
    }

    public static bool AreAdjacent(IZone z1, IZone z2)
    {
        foreach (var c1 in z1)
        {
            if (CellUtility.AreAdjacent(z2, c1)) return true;
        }

        return false;
    }

    public static IEnumerable<Cell> GetNeighbors(Cell cell, int rowCount, int colCount)
    {
        return GetNeighbors(cell.Row, cell.Column, rowCount, colCount);
    }
    
    public static IEnumerable<Cell> GetNeighbors(int row, int col, int rowCount, int colCount)
    {
        if (row > 0)
        {
            yield return new Cell(row - 1, col);
            if (col > 0) yield return new Cell(row - 1, col - 1);
        }

        if (col > 0)
        {
            yield return new Cell(row, col - 1);
            if (row < rowCount - 1) yield return new Cell(row + 1, col - 1);
        }

        if (row < rowCount - 1)
        {
            yield return new Cell(row + 1, col);
            if (col < colCount - 1) yield return new Cell(row + 1, col + 1);
        }

        if (col < colCount - 1)
        {
            yield return new Cell(row, col + 1);
            if (row > 0) yield return new Cell(row - 1, col + 1);
        }
    }
    
    public static IEnumerable<Cell> SharedNeighboringCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        if (cells.Count == 0) yield break;

        foreach (var neighbor in GetNeighbors(cells[0], tectonic.RowCount, tectonic.ColumnCount))
        {
            bool ok = true;

            for (int i = 1; i < cells.Count; i++)
            {
                if (!AreNeighbors(neighbor, cells[i]) || cells[i] == neighbor)
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return neighbor;
        }
    }
    
    public static IEnumerable<Cell> SharedNeighboringCells(IReadOnlyTectonic tectonic, Cell one, Cell two)
    {
        foreach (var neighbor in GetNeighbors(one, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (AreNeighbors(two, neighbor)) yield return neighbor;
        }
    }

    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyTectonic tectonic, Cell one, Cell two)
    {
        var z1 = tectonic.GetZone(one);
        var z2 = tectonic.GetZone(two);
        
        if (z1.Equals(z2))
        {
            foreach (var cell in z1)
            {
                if (cell != one && cell != two) yield return cell;
            }
        }
        else
        {
            foreach (var cell in z1)
            {
                if (cell != one && AreNeighbors(cell, two)) yield return cell;
            }
        
            foreach (var cell in z2)
            {
                if (cell != two && AreNeighbors(cell, one)) yield return cell;
            }
        }
        
        foreach (var neighbor in GetNeighbors(one, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (!z1.Contains(neighbor) && !z2.Contains(neighbor) && AreNeighbors(neighbor, two)) yield return neighbor;
        }
    }

    public static IEnumerable<Cell> SeenCells(IReadOnlyTectonic tectonic, Cell cell)
    {
        var zone = tectonic.GetZone(cell);
        foreach (var c in zone)
        {
            if (c != cell) yield return c;
        }

        foreach (var c in GetNeighbors(cell, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (!zone.Contains(c)) yield return c;
        }
    }

    public static IEnumerable<Cell> SharedSeenCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        return cells.Count switch
        {
            0 => Enumerable.Empty<Cell>(),
            1 => SeenCells(tectonic, cells[0]),
            _ => CheckedSharedSeenCells(tectonic, cells)
        };
    }

    private static IEnumerable<Cell> CheckedSharedSeenCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        foreach (var cell in SharedSeenCells(tectonic, cells[0], cells[1]))
        {
            bool ok = true;
            
            for (int i = 2; i < cells.Count; i++)
            {
                if (cells[i] == cell || (!AreNeighbors(cells[i], cell) && !tectonic.GetZone(cells[i]).Contains(cell)))
                {
                    ok = false;
                    break;
                }
            }

            if (ok) yield return cell;
        }
    }

    public static IEnumerable<CellPossibility> SharedSeenPossibilities(ITectonicSolverData solverData,
        CellPossibility cp1, CellPossibility cp2)
    {
        var c1 = cp1.ToCell();
        var c2 = cp2.ToCell();
        if (cp1.Possibility == cp2.Possibility)
        {
            foreach (var cell in SharedSeenCells(solverData.Tectonic, c1, c2))
            {
                yield return new CellPossibility(cell, cp1.Possibility);
            }
        }
        else if (c1 == c2)
        {
            foreach (var p in solverData.PossibilitiesAt(c1).EnumeratePossibilities())
            {
                if (p != cp1.Possibility && p != cp2.Possibility)
                    yield return new CellPossibility(c1, p);
            }
        }
    }
}