using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.AlmostLockedSets;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public class SudokuHighlighterTranslator : ISudokuHighlighter
{
    private readonly ISudokuSolverDrawer _drawer;
    private readonly Settings _settings;

    public SudokuHighlighterTranslator(ISudokuSolverDrawer drawer, Settings settings)
    {
        _drawer = drawer;
        _settings = settings;
    }

    public void Translate(IHighlightable<ISudokuHighlighter> lighter)
    {
        lighter.Highlight(this);
        _drawer.Refresh();
    }

    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _drawer.FillPossibility(row, col, possibility, coloration);
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        _drawer.EncirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _drawer.FillCell(row, col, coloration);
    }

    public void EncircleCell(int row, int col)
    {
        _drawer.EncircleCell(row, col);
    }

    public void EncircleHouse(House house, ChangeColoration coloration)
    {
        var extremities = house.GetExtremities();
        _drawer.EncircleRectangle(extremities.Item1.Row, extremities.Item1.Column,
            extremities.Item2.Row, extremities.Item2.Column, coloration);
    }

    public void HighlightElement(ISudokuElement element, ChangeColoration coloration)
    {
        if (ChangeColorationUtility.IsOff(coloration) && element is PointingRow or PointingColumn or CellsPossibility)
        {
            foreach (var cp in element.EnumerateCellPossibility())
            {
                HighlightPossibility(cp.Possibility, cp.Row, cp.Column, coloration);
            }

            return;
        }
        
        switch (element)
        {
            case CellPossibility cp :
                HighlightPossibility(cp.Possibility, cp.Row, cp.Column, coloration);
                break;
            case PointingRow pr :
                var minCol = 9;
                var maxCol = -1;
                foreach (var cell in pr.EnumerateCell())
                {
                    if (cell.Column < minCol) minCol = cell.Column;
                    if (cell.Column > maxCol) maxCol = cell.Column;
                }
                
                _drawer.EncircleRectangle(pr.Row, minCol, pr.Possibility, pr.Row,
                    maxCol, pr.Possibility, coloration);
                break;
            case PointingColumn pc :
                var minRow = 9;
                var maxRow = -1;
                foreach (var cell in pc.EnumerateCell())
                {
                    if (cell.Row < minRow) minRow = cell.Row;
                    if (cell.Row > maxRow) maxRow = cell.Row;
                }

                _drawer.EncircleRectangle(minRow, pc.Column, pc.Possibility, maxRow,
                    pc.Column, pc.Possibility, coloration);
                break;
            case NakedSet ans :
                _drawer.DelimitPossibilityPatch(ans.EveryCellPossibility(), coloration);
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        if (_settings.ShowSameCellLinks && from.ToCell() == to.ToCell()) return;
        
        _drawer.CreateLink(from.Row, from.Column, from.Possibility, to.Row, to.Column,
            to.Possibility, linkStrength, _settings.LinkOffsetSidePriority);
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        if (_settings.ShowSameCellLinks && from is CellPossibility cp1 && to is CellPossibility cp2
            && cp1.ToCell() == cp2.ToCell()) return;
        
        if (linkStrength == LinkStrength.Strong && (from is NakedSet || to is NakedSet)) return;
        
        var possibilitiesFrom = from.EveryPossibilities();
        var possibilitiesTo = to.EveryPossibilities();
        if (possibilitiesFrom.Count == 0 || possibilitiesTo.Count == 0) return;

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

                        var dist = CellUtility.Distance(cellF.Cell, possF, cellT.Cell, possT);
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

        CreateLink(minCells[0], minCells[1], linkStrength);
    }
}