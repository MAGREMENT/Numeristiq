using System.Collections.Generic;
using Model;
using Model.Helpers.Changes;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudoku.Solve;

public interface ISudokuDrawer
{
    void PutCursorOn(Cell cell);
    void ClearCursor();
    void ClearNumbers();
    void ClearHighlights();
    void ShowSolution(int row, int col, int number);
    void ShowPossibilities(int row, int col, IEnumerable<int> possibilities);
    void SetClue(int row, int column, bool isClue);
    void FillPossibility(int row, int col, int possibility, ChangeColoration coloration);
    void FillCell(int row, int col, ChangeColoration coloration);
    void EncirclePossibility(int row, int col, int possibility);
    public void EncirclePossibilityPatch(CellPossibility[] cps, ChangeColoration coloration);
    void EncircleCell(int row, int col);
    void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration coloration);
    void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration);
    void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority);
    void Refresh();
}