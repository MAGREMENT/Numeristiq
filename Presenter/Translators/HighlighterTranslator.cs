using Global;
using Global.Enums;
using Model;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.AlmostLockedSets;
using Model.Solver.StrategiesUtility.Graphs;
using Presenter.Solver;

namespace Presenter.Translators;

public class HighlighterTranslator : IHighlighter
{
    private readonly ISolverDrawer _drawer;
    private readonly ISolverSettings _settings;

    public HighlighterTranslator(ISolverDrawer drawer, ISolverSettings settings)
    {
        _drawer = drawer;
        _settings = settings;
    }

    public void Translate(IHighlightable lighter)
    {
        lighter.Highlight(this);
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

    public void EncircleRectangle(CellPossibility from, CellPossibility to, ChangeColoration coloration)
    {
        _drawer.EncircleRectangle(from.Row, from.Column, from.Possibility, to.Row,
            to.Column, to.Possibility, coloration);
    }

    public void EncircleRectangle(CoverHouse house, ChangeColoration coloration)
    {
        Cell min = default;
        Cell max = default;
        switch (house.Unit)
        {
            case Unit.Row :
                min = new Cell(house.Number, 0);
                max = new Cell(house.Number, 8); 
                break;
            case Unit.Column :
                min = new Cell(0, house.Number);
                max = new Cell(8, house.Number);
                break;
            case Unit.MiniGrid :
                var sRow = house.Number / 3 * 3;
                var sCol = house.Number % 3 * 3;
                min = new Cell(sRow, sCol);
                max = new Cell(sRow + 2, sCol + 2);
                break;
        }

        _drawer.EncircleRectangle(min.Row, min.Column, max.Row, max.Column, coloration);
    }

    public void HighlightLinkGraphElement(ISudokuElement element, ChangeColoration coloration)
    {
        if (ChangeColorationUtility.IsOff(coloration) && element is PointingRow or PointingColumn or CellsPossibility)
        {
            foreach (var cp in element.EveryCellPossibility())
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
                foreach (var cell in pr.EveryCell())
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
                foreach (var cell in pc.EveryCell())
                {
                    if (cell.Row < minRow) minRow = cell.Row;
                    if (cell.Row > maxRow) maxRow = cell.Row;
                }

                _drawer.EncircleRectangle(minRow, pc.Column, pc.Possibility, maxRow,
                    pc.Column, pc.Possibility, coloration);
                break;
            case NakedSet ans :
                _drawer.EncircleCellPatch(ans.EveryCell(), coloration);
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        if (!_settings.ShowSameCellLinks && from.ToCell() == to.ToCell()) return;
        
        _drawer.CreateLink(from.Row, from.Column, from.Possibility, to.Row, to.Column,
            to.Possibility, linkStrength, _settings.SidePriority);
    }

    public void CreateLink(ISudokuElement from, ISudokuElement to, LinkStrength linkStrength)
    {
        if (!_settings.ShowSameCellLinks && from is CellPossibility cp1 && to is CellPossibility cp2
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
                if (possibilitiesFrom.Equals(possibilitiesTo)) possibilitySearch = possibilitiesFrom.First();
                else possibilitySearch = -1;
                break;
            case (1, > 1) :
                first = possibilitiesFrom.First();
                if (possibilitiesTo.Peek(first)) possibilitySearch = first;
                else possibilitySearch = -1;
                break;
            case (> 1, 1) :
                first = possibilitiesTo.First();
                if (possibilitiesFrom.Peek(first)) possibilitySearch = first;
                else possibilitySearch = -1;
                break;
            default :
                possibilitySearch = -1;
                break;
        }

        var minCells = new CellPossibility[2];
        var minDist = double.MaxValue;

        foreach (var cellF in from.EveryCellPossibilities())
        {
            foreach (var cellT in to.EveryCellPossibilities())
            {
                foreach (var possF in cellF.Possibilities)
                {
                    if (possibilitySearch != -1 && possF != possibilitySearch) continue;

                    foreach (var possT in cellT.Possibilities)
                    {
                        if (possibilitySearch != -1 && possT != possibilitySearch) continue;

                        var dist = Cells.Distance(cellF.Cell, possF, cellT.Cell, possT);
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