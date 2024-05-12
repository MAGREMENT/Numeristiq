using Model.Kakuros;

namespace DesktopApplication.Presenter.Kakuros.Solve;

public interface IKakuroSolverDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }

    void ClearPresence();
    void SetPresence(int row, int col, bool value);
    void SetPresence(int row, int col, Orientation orientation, bool value);
    void RedrawLines();
    void Refresh();
}