using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Core.Highlighting;

public interface INonogramHighlighter
{
    void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
    void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color);
    void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
}

public interface IBinairoHighlighter
{
    void HighlightCell(int row, int col, StepColor color);
    void HighlightCell(Cell c, StepColor color) => HighlightCell(c.Row, c.Column, color);
    void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color);
}