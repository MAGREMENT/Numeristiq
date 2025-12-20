using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;
using Model.Utility;

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

    public void EncircleRectangle(CellPossibility topLeft, CellPossibility bottomRight, StepColor color)
    {
        _drawer.EncircleRectangle(topLeft, bottomRight, color);
    }

    public void EncircleRectangle(Cell topLeft, Cell bottomRight, StepColor color)
    {
        _drawer.EncircleRectangle(topLeft.Row, topLeft.Column, bottomRight.Row, bottomRight.Column, color);
    }

    public void DelimitPossibilityPatch(CellPossibility[] cps, StepColor color)
    {
        _drawer.DelimitPossibilityPatch(cps, color);
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        if (!_settings.ShowSameCellLinks.Get().ToBool() && from.ToCell() == to.ToCell()) return;
        
        _drawer.CreateLink(from.Row, from.Column, from.Possibility, to.Row, to.Column,
            to.Possibility, linkStrength);
    }
}