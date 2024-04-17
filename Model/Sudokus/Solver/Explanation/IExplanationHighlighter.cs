using Model.Sudokus.Solver.StrategiesUtility;
using Model.Utility;

namespace Model.Sudokus.Solver.Explanation;

public interface IExplanationHighlighter
{
    void ShowCell(Cell c);
    void ShowCellPossibility(CellPossibility cp);
    void ShowCoverHouse(House ch);
}