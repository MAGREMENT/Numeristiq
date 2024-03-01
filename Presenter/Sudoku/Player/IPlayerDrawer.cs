using Model;
using Model.Sudoku;
using Model.Sudoku.Player;
using Model.Utility;

namespace Presenter.Sudoku.Player;

public interface IPlayerDrawer
{
    void ShowPossibilities(int row, int col, int[] possibilities, PossibilitiesLocation location);
    void ShowNumber(int row, int col, int number, CellColor color);
    void ClearNumbers();
    void PutCursorOn(HashSet<Cell> cells);
    void ClearCursor();
    void Refresh();
    void ClearDrawings();
    void HighlightCell(int row, int col, HighlightColor color);
    void HighlightCell(int row, int col, HighlightColor[] colors, double startAngle, RotationDirection direction);
}