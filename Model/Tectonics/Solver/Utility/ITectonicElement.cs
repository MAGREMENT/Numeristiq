using System.Collections.Generic;
using Model.Utility;

namespace Model.Tectonics.Solver.Utility;

public interface ITectonicElement
{
    IEnumerable<Cell> EnumerateCells();
    IEnumerable<CellPossibility> EnumerateCellPossibility();
}