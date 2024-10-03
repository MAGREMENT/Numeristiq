using System;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus;
using Model.Sudokus.Solver.PossibilitySets;
using Model.Sudokus.Solver.Utility;
using Model.Utility;
using Model.Utility.BitSets;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public class SudokuHighlighterTranslator : IHighlighterTranslator<ISudokuHighlighter>, ISudokuHighlighter
{
    private readonly ISudokuSolverDrawer _drawer;
    private readonly Settings _settings;

    public SudokuHighlighterTranslator(ISudokuSolverDrawer drawer, Settings settings)
    {
        _drawer = drawer;
        _settings = settings;
    }

    public void Translate(IHighlightable<ISudokuHighlighter> lighter, bool clear)
    {
        if(clear) _drawer.ClearHighlights();
        lighter.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        _drawer.FillPossibility(row, col, possibility, color);
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        _drawer.EncirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        _drawer.FillCell(row, col, color);
    }

    public void EncircleCell(int row, int col)
    {
        _drawer.EncircleCell(row, col);
    }

    public void EncircleHouse(House house, StepColor color)
    {
        var extremities = house.GetExtremities();
        _drawer.EncircleRectangle(extremities.Item1.Row, extremities.Item1.Column,
            extremities.Item2.Row, extremities.Item2.Column, color);
    }

    public void HighlightElement(ISudokuElement element, StepColor color)
    {
        if (ChangeColorationUtility.IsOff(color) && element is PointingRow or PointingColumn or CellsPossibility)
        {
            foreach (var cp in element.EnumerateCellPossibility())
            {
                HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
            }

            return;
        }
        
        switch (element)
        {
            case CellPossibility cp :
                HighlightPossibility(cp.Possibility, cp.Row, cp.Column, color);
                break;
            case PointingRow pr :
                var cols = pr.FindMinMaxColumns();
                
                _drawer.EncircleRectangle(pr.Row, cols.Min, pr.Possibility, pr.Row,
                    cols.Max, pr.Possibility, color);
                break;
            case PointingColumn pc :
                var rows = pc.FindMinMaxRows();

                _drawer.EncircleRectangle(rows.Min, pc.Column, pc.Possibility, rows.Max,
                    pc.Column, pc.Possibility, color);
                break;
            case CellsPossibility csp :
                _drawer.DelimitPossibilityPatch(csp.EveryCellPossibility(), color);
                break;
            case IPossibilitySet ans :
                _drawer.DelimitPossibilityPatch(ans.EveryCellPossibility(), color);
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        if (!_settings.ShowSameCellLinks.Get().ToBool() && from.ToCell() == to.ToCell()) return;
        
        _drawer.CreateLink(from.Row, from.Column, from.Possibility, to.Row, to.Column,
            to.Possibility, linkStrength);
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        if (!_settings.ShowSameCellLinks.Get().ToBool() && from is CellPossibility cp1 && to is CellPossibility cp2
            && cp1.ToCell() == cp2.ToCell()) return;
        
        if (linkStrength == LinkStrength.Strong && (from is IPossibilitySet || to is IPossibilitySet)) return;

        var closest = FindClosest(from, to);
        if (closest is null) return;

        CreateLink(closest.Value.Item1, closest.Value.Item2, linkStrength);
    }

    public void CreateLink(IPossibilitySet from, IPossibilitySet to, int link)
    {
        var (m1, m2) = FindClosest(from, to, link);
                
        CreateLink(m1, m2, LinkStrength.Strong);
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

                        var dist = Distance(cellF.Cell, possF, cellT.Cell, possT);
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
                var dist = Distance(cell1, link, cell2, link);

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
    
    private static double Distance(Cell oneCell, int onePoss, Cell twoCell, int twoPoss)
    {
        var oneX = oneCell.Column * 3 + onePoss % 3;
        var oneY = oneCell.Row * 3 + onePoss / 3;

        var twoX = twoCell.Column * 3 + twoPoss % 3;
        var twoY = twoCell.Row * 3 + twoPoss / 3;

        var dx = twoX - oneX;
        var dy = twoY - oneY;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}