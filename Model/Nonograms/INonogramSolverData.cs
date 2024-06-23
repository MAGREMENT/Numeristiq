using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;
using Model.Kakuros;
using Model.Utility;

namespace Model.Nonograms;

public interface INonogramSolverData
{
    DichotomousChangeBuffer<IUpdatableDichotomousSolvingState, object> ChangeBuffer { get; }
    IReadOnlyNonogram Nonogram { get; }
    bool IsAvailable(int row, int col);
    IEnumerable<LineSpace> EnumerateSpaces(Orientation orientation, int index);
}