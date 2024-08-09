using System.Collections.Generic;
using Model.Core.Changes;
using Model.Utility;

namespace DesktopApplication.Presenter.Nonograms.Solve;

public interface INonogramDrawer : IDrawer
{
    void SetRows(IEnumerable<IEnumerable<int>> rows);
    void SetColumns(IEnumerable<IEnumerable<int>> cols);
    void SetSolution(int row, int col);
    void ClearSolutions();
    void SetUnavailable(int row, int col);
    void ClearUnavailable();
    void EncircleCells(HashSet<Cell> cells, StepColor color);
    void HighlightValues(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation);
    void EncircleSection(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation);
}