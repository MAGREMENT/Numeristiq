using System;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Sudokus.Solver.Highlighting;

public interface ISudokuHighlighter : INumericSolvingStateHighlighter
{
    public void EncirclePossibility(int possibility, int row, int col);

    public void EncircleCell(int row, int col);

    public void EncircleRectangle(CellPossibility topLeft, CellPossibility bottomRight, StepColor color);

    public void EncircleRectangle(Cell topLeft, Cell bottomRight, StepColor color);

    public void DelimitPossibilityPatch(CellPossibility[] cps, StepColor color);
}

public static class ISudokuHighlighterExtensions
{
    public static void EncircleCell(this ISudokuHighlighter h, Cell coord)
    {
        h.EncircleCell(coord.Row, coord.Column);
    }
    
    public static void EncirclePossibility(this ISudokuHighlighter h, CellPossibility coord)
    {
        h.EncirclePossibility(coord.Possibility, coord.Row, coord.Column);
    }
    
    public static void EncircleHouse(this ISudokuHighlighter h, House house, StepColor color)
    {
        var extremities = house.GetExtremities();
        h.EncircleRectangle(extremities.Item1, extremities.Item2, color);
    }
    
    public static void CreateLink(this ISudokuHighlighter h, ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        if (linkStrength == LinkStrength.Strong && (from is IPossibilitySet || to is IPossibilitySet)) return;

        var closest = FindClosest(from, to);
        if (closest is null) return;

        h.CreateLink(closest.Value.Item1, closest.Value.Item2, linkStrength);
    }
    
    public static void CreateLink(this ISudokuHighlighter h, IPossibilitySet from, IPossibilitySet to, int link)
    {
        var (m1, m2) = FindClosest(from, to, link);
                
        h.CreateLink(m1, m2, LinkStrength.Strong);
    }
    
    public static void HighlightElement(this ISudokuHighlighter h, ISudokuElement element, StepColor color)
    {
        if (ChangeColorationUtility.IsOff(color) && element is PointingRow or PointingColumn or CellsPossibility)
        {
            foreach (var cp in element.EnumerateCellPossibility())
            {
                h.HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
            }

            return;
        }
        
        switch (element)
        {
            case CellPossibility cp :
                h.HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
                break;
            case PointingRow pr :
                var cols = pr.FindMinMaxColumns();
                
                h.EncircleRectangle(new CellPossibility(pr.Row, cols.Min, pr.Possibility), new CellPossibility(pr.Row,
                    cols.Max, pr.Possibility), color);
                break;
            case PointingColumn pc :
                var rows = pc.FindMinMaxRows();

                h.EncircleRectangle(new CellPossibility(rows.Min, pc.Column, pc.Possibility), new CellPossibility(rows.Max,
                    pc.Column, pc.Possibility), color);
                break;
            case CellsPossibility csp :
                h.DelimitPossibilityPatch(csp.EveryCellPossibility(), color);
                break;
            case IPossibilitySet ans :
                h.DelimitPossibilityPatch(ans.EveryCellPossibility(), color);
                break;
        }
    }
    
    private static (CellPossibility, CellPossibility)? FindClosest(ISudokuElement from, ISudokuElement to)
    {
        var possibilitiesFrom = from.EveryPossibilities();
        var possibilitiesTo = to.EveryPossibilities();
        if (possibilitiesFrom.Count == 0 || possibilitiesTo.Count == 0) return null;

        int possibilitySearch;
        int first;
        switch (possibilitiesFrom.Count, possibilitiesTo.Count)
        {
            case(1, 1) :
                if (possibilitiesFrom.Equals(possibilitiesTo)) possibilitySearch = possibilitiesFrom.FirstPossibility();
                else possibilitySearch = -1;
                break;
            case (1, > 1) :
                first = possibilitiesFrom.FirstPossibility();
                if (possibilitiesTo.Contains(first)) possibilitySearch = first;
                else possibilitySearch = -1;
                break;
            case (> 1, 1) :
                first = possibilitiesTo.FirstPossibility();
                if (possibilitiesFrom.Contains(first)) possibilitySearch = first;
                else possibilitySearch = -1;
                break;
            default :
                possibilitySearch = -1;
                break;
        }

        var minCells = new CellPossibility[2];
        var minDist = double.MaxValue;

        foreach (var cellF in from.EnumerateCellPossibilities())
        {
            foreach (var cellT in to.EnumerateCellPossibilities())
            {
                foreach (var possF in cellF.Possibilities.EnumeratePossibilities())
                {
                    if (possibilitySearch != -1 && possF != possibilitySearch) continue;

                    foreach (var possT in cellT.Possibilities.EnumeratePossibilities())
                    {
                        if (possibilitySearch != -1 && possT != possibilitySearch) continue;

                        var dist = CellPossibilityUtility.Distance(new CellPossibility(cellF.Cell, possF), 
                            new CellPossibility(cellT.Cell, possT));
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minCells[0] = new CellPossibility(cellF.Cell, possF);
                            minCells[1] = new CellPossibility(cellT.Cell, possT);
                        }
                    }
                }
            }
        }

        return (minCells[0], minCells[1]);
    }

    private static (CellPossibility, CellPossibility) FindClosest(IPossibilitySet from, IPossibilitySet to, int link)
    {
        var minDistance = double.MaxValue;
        var minCells = new CellPossibility[2];
                
        foreach (var cell1 in from.EnumerateCells(link))
        {
            foreach (var cell2 in to.EnumerateCells(link))
            {
                var dist = CellPossibilityUtility.Distance(new CellPossibility(cell1, link), 
                        new CellPossibility(cell2, link));

                if (dist < minDistance)
                {
                    minDistance = dist;
                    minCells[0] = new CellPossibility(cell1, link);
                    minCells[1] = new CellPossibility(cell2, link);
                }
            }
        }
        
        return (minCells[0], minCells[1]);
    }
}