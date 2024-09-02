using System.Collections.Generic;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramDrawer : IDrawer, INonogramHighlighter
{
    void SetRows(IEnumerable<IEnumerable<int>> rows);
    void SetColumns(IEnumerable<IEnumerable<int>> cols);
    void SetSolution(int row, int col);
    void ClearSolutions();
    void SetUnavailable(int row, int col);
    void ClearUnavailable();
}