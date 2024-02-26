using Model.Sudoku.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudoku.Solver.Explanation;

public interface IExplanationHighlighter
{
    void ShowCell(Cell c);
    void ShowCellPossibility(CellPossibility cp);
    void ShowCoverHouse(CoverHouse ch);
}