using Model.Sudokus.Solver.Utility;
using Model.Utility;

namespace Model.Core.Explanation;

public interface IExplanationHighlighter
{
    void ShowCell(Cell c, ExplanationColor color);
    void ShowCellPossibility(CellPossibility cp, ExplanationColor color);
    void ShowCoverHouse(House ch, ExplanationColor color);
}