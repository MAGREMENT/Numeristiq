using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus;

public interface ISudokuDrawer : IDrawer
{
    void PutCursorOn(Cell cell);
    void ClearCursor();
    void ClearNumbers();
    void ShowSolution(int row, int col, int number);
    void SetClue(int row, int col, bool isClue);
}