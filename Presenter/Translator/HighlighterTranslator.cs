using Global;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Presenter.Translator;

public class HighlighterTranslator : IHighlightable
{
    private readonly ISolverView _view;

    public HighlighterTranslator(ISolverView view)
    {
        _view = view;
    }

    public void Translate(HighlightManager manager)
    {
        manager.Highlight(this);
    }

    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _view.FillPossibility(row, col, possibility, coloration);
    }

    public void EncirclePossibility(int possibility, int row, int col)
    {
        _view.EncirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _view.FillCell(row, col, coloration);
    }

    public void EncircleCell(int row, int col)
    {
        _view.EncircleCell(row, col);
    }

    public void HighlightLinkGraphElement(ILinkGraphElement element, ChangeColoration coloration)
    {
        switch (element)
        {
            case CellPossibility cp :
                HighlightPossibility(cp.Possibility, cp.Row, cp.Col, coloration);
                break;
            case PointingRow pr :
                var minCol = 9;
                var maxCol = -1;
                foreach (var cell in pr.EveryCell())
                {
                    if (cell.Col < minCol) minCol = cell.Col;
                    if (cell.Col > maxCol) maxCol = cell.Col;
                }
                
                _view.EncircleRectangle(pr.Row, minCol, pr.Possibility, pr.Row,
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

                _view.EncircleRectangle(minRow, pc.Column, pc.Possibility, maxRow,
                    pc.Column, pc.Possibility, coloration);
                break;
            case AlmostNakedSet ans :
                _view.EncircleCellPatch(ans.EveryCell(), coloration);
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _view.CreateLink(from.Row, from.Col, from.Possibility, to.Row, to.Col,
            to.Possibility, linkStrength);
    }

    public void CreateLink(ILinkGraphElement from, ILinkGraphElement to, LinkStrength linkStrength)
    {
        if (from is CellPossibility && to is AlmostNakedSet) return;
        
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

                        var dist = GetDistance(cellF.Cell, possF, cellT.Cell, possT);
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

    private double GetDistance(Cell oneCell, int onePoss, Cell twoCell, int twoPoss)
    {
        var oneX = oneCell.Col * 3 + onePoss % 3;
        var oneY = oneCell.Row * 3 + onePoss / 3;

        var twoX = twoCell.Col * 3 + twoPoss % 3;
        var twoY = twoCell.Row * 3 + twoPoss / 3;

        var dx = twoX - oneX;
        var dy = twoY - oneY;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}