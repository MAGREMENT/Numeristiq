using System.Collections.Generic;
using Model.Core.Changes;
using Model.Utility;

namespace Model.Core.Highlighting;

public interface INonogramHighlighter
{
    void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
    void EncircleCells(IEnumerable<Cell> cells, StepColor color);
    void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, StepColor color);
}