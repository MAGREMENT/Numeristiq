using System.Collections.Generic;
using Model.Core.Changes;
using Model.Utility;

namespace Model.Core.Highlighting;

public interface INonogramHighlighter
{
    void HighlightValues(Orientation orientation, int unit, int startIndex, int endIndex, ChangeColoration color);
    void EncircleCells(IEnumerable<Cell> cells, ChangeColoration color);
    void EncircleLineSection(Orientation orientation, int unit, int startIndex, int endIndex, ChangeColoration color);
}