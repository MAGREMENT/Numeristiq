using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Nonograms.Solver;

public interface INonogramHighlighter
{
    void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
    void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color);
    void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
}