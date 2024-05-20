using System.Collections.Generic;
using Model.Kakuros;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolverDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }

    void ClearPresence();
    void SetPresence(int row, int col, bool value);
    void SetPresence(int row, int col, Orientation orientation, bool value);
    void ClearNumbers();
    void SetAmount(int row, int col, int n, Orientation orientation);
    void ReplaceAmount(int row, int col, int n, Orientation orientation);
    void SetSolution(int row, int col, int n);
    void SetPossibilities(int row, int col, IEnumerable<int> poss);
    void ClearCursor();
    void PutCursorOnNumberCell(int row, int col);
    void PutCursorOnAmountCell(int row, int col, Orientation orientation);
    void RedrawLines();
    void Refresh();
}