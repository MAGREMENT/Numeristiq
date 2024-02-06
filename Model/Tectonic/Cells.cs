using System;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonic;

public static class Cells
{
    public static bool AreNeighbors(Cell c1, Cell c2)
    {
        return Math.Abs(c1.Row - c2.Row) <= 1 && Math.Abs(c1.Column - c2.Column) <= 1;
    }
    
    public static IEnumerable<Cell> SharedNeighboringCells(IReadOnlyTectonic tectonic, IReadOnlyList<Cell> cells)
    {
        if (cells.Count == 0) yield break;

        foreach (var neighbor in tectonic.GetNeighbors(cells[0]))
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
}