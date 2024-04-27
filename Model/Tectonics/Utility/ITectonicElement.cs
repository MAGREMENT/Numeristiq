using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonics.Utility;

public interface ITectonicElement
{
    IEnumerable<Cell> EnumerateCell();
    IEnumerable<CellPossibility> EnumerateCellPossibility();
}