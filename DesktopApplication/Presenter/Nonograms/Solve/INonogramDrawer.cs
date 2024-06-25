using System.Collections.Generic;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramDrawer
{
    void Refresh();
    void SetRows(IEnumerable<IEnumerable<int>> rows);
    void SetColumns(IEnumerable<IEnumerable<int>> cols);
    void SetSolution(int row, int col);
    void ClearSolutions();
    void SetUnavailable(int row, int col);
    void ClearUnavailable();
}