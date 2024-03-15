using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

public static class Cells
{
    public static bool AreNeighbors(Cell c1, Cell c2)
    {
        return c1 != c2 && Math.Abs(c1.Row - c2.Row) <= 1 && Math.Abs(c1.Column - c2.Column) <= 1;
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
        foreach (var neighbor in GetNeighbors(one, tectonic.RowCount, tectonic.ColumnCount))
        {
            if (AreNeighbors(neighbor, two)) yield return neighbor;
        }

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

        
    }
}