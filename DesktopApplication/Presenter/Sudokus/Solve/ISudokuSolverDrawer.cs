using System.Collections.Generic;
using Model.Core.Changes;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace DesktopApplication.Presenter.Sudokus.Solve;

public interface ISudokuSolverDrawer : ISudokuDrawer
{
    bool FastPossibilityDisplay { set; }
    void ShowPossibilities(int row, int col, IEnumerable<int> possibilities);
    void FillPossibility(int row, int col, int possibility, ChangeColoration coloration);
    void FillCell(int row, int col, ChangeColoration coloration);
    void DelimitPossibilityPatch(CellPossibility[] cps, ChangeColoration coloration);
    void EncirclePossibility(int row, int col, int possibility);
    void EncircleCell(int row, int col);
    void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration coloration);
    void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration coloration);
    void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority);
}