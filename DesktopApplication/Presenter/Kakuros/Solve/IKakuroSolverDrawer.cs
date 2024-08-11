using System.Collections.Generic;
using Model.Utility;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolverDrawer : IDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }
    bool FastPossibilityDisplay { set; }

    void ClearPresence();
    void SetPresence(int row, int col, bool value);
    void ClearNumbers();
    void ClearAmounts();
    void AddAmount(int row, int col, int n, Orientation orientation);
    void ReplaceAmount(int row, int col, int n, Orientation orientation);
    void SetSolution(int row, int col, int n);
    void SetPossibilities(int row, int col, IEnumerable<int> poss);
    void ClearCursor();
    void PutCursorOnNumberCell(int row, int col);
    void PutCursorOnAmountCell(int row, int col, Orientation orientation);
}