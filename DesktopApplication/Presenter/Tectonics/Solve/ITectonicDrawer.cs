using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.Presenter.Tectonics.Solve;

public interface ITectonicDrawer
{
    int RowCount { set; }
    int ColumnCount { set; }

    void Refresh();
    void ClearNumbers();
    void ShowSolution(int row, int column, int number);
    void ShowPossibilities(int row, int column, IEnumerable<int> possibilities);
    void SetClue(int row, int column, bool isClue);
    void ClearBorderDefinitions();
    void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin);
    void PutCursorOn(Cell cell);
    void PutCursorOn(IContainingEnumerable<Cell> cells);
    void ClearCursor();
    void ClearHighlights();
    void FillPossibility(int row, int col, int possibility, ChangeColoration coloration);
    void FillCell(int row, int col, ChangeColoration coloration);
    void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority);
}

public enum BorderDirection
{
    Horizontal, Vertical
}