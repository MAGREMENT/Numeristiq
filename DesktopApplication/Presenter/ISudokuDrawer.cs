using Model.Utility;

namespace DesktopApplication.Presenter;

public interface ISudokuDrawer
{
    void PutCursorOn(Cell cell);
    void ClearCursor();
    void ClearNumbers();
    void ClearHighlights();
    void ShowSolution(int row, int col, int number);
    void Refresh();
}