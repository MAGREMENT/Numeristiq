using Model.Core.Changes;

namespace DesktopApplication.Presenter.Binairos.Solve;

public interface IBinairoDrawer : IDrawer
{
    LinkOffsetSidePriority LinkOffsetSidePriority { set; }
    bool AreSolutionNumbers { set; }
    
    int RowCount { set; }
    int ColumnCount { set; }

    void ClearSolutions();
    void ShowSolution(int solution, int row, int col);
    void SetClue(int row, int col, bool isClue);
    void HighlightCell(int row, int col, StepColor color);
}