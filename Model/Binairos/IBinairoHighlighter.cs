using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Binairos;

public interface IBinairoHighlighter
{
    void HighlightCell(int row, int col, StepColor color);
    void HighlightCell(Cell c, StepColor color) => HighlightCell(c.Row, c.Column, color);
    void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color);
    void CreateLink(Cell from, Cell to);
    void SimulateSolution(int solution, int row, int col);
}