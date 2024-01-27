using Global;
using Model.SudokuSolving.Solver.StrategiesUtility;

namespace Model.SudokuSolving.Solver.Explanation;

public interface IExplanationShower
{
    void ShowCell(Cell c);
    void ShowCellPossibility(CellPossibility cp);
    void ShowCoverHouse(CoverHouse ch);
}