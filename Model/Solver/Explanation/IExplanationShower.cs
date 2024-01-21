using Global;
using Model.Solver.StrategiesUtility;

namespace Model.Solver.Explanation;

public interface IExplanationShower
{
    void ShowCell(Cell c);
    void ShowCellPossibility(CellPossibility cp);
    void ShowCoverHouse(CoverHouse ch);
}