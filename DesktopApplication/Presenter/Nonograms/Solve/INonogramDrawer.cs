using System.Collections.Generic;
using Model.Core.Changes;
using Model.Utility;

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
    void ClearHighlights();
    void EncircleCells(HashSet<Cell> cells, StepColor color);
    void HighlightValues(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation);
    void EncircleSection(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation);
}