using DesktopApplication.Presenter.Tectonics.Solve;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.YourPuzzles;

public interface IYourPuzzleDrawer : IDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }
    public void PutCursorOn(IContainingEnumerable<Cell> cells);
    public void ClearCursor();
    public void ClearBorderDefinitions();
    public void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin);
    public void ClearGreaterThanSigns();
    public void AddGreaterThanSign(Cell smaller, Cell greater);
}