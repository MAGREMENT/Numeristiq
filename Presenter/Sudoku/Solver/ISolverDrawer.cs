using Model.Sudoku;
using Model.Sudoku.Solver.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Presenter.Sudoku.Solver;

public interface ISolverDrawer
{
    void PutCursorOn(Cell cell);
    void ClearCursor();
    void ClearNumbers();
    public void ClearDrawings();
    void ShowSolution(int row, int col, int number, CellColor color);
    void ShowPossibilities(int row, int col, int[] possibilities, CellColor color);
    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration);
    public void FillCell(int row, int col, ChangeColoration coloration);
    public void EncirclePossibility(int row, int col, int possibility);
    public void EncircleCell(int row, int col);
    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration coloration);
    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration);
    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration);
    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority);
    public void Refresh();
}