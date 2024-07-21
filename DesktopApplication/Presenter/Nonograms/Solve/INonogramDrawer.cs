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
    void HighlightHorizontalValues(int row, int startIndex, int endIndex, StepColor color);
    void HighlightVerticalValues(int col, int startIndex, int endIndex, StepColor color);
    void EncircleRowSection(int row, int startIndex, int endIndex, StepColor color);
    void EncircleColumnSection(int col, int startIndex, int endIndex, StepColor color);
}