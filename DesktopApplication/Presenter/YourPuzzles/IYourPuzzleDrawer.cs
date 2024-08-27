using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.YourPuzzles;

public interface IYourPuzzleDrawer : IDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }
    public void PutCursorOn(IContainingEnumerable<Cell> cells);
    public void ClearCursor();
}