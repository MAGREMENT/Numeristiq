using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms;

public interface INonogramSolverData
{
    DichotomousChangeBuffer<IUpdatableDichotomousSolvingState, object> ChangeBuffer { get; }
    IReadOnlyNonogram Nonogram { get; }
    bool IsAvailable(int row, int col);
    IReadOnlyList<LineSpace> HorizontalSpacesFor(int index);
    IReadOnlyList<LineSpace> VerticalSpacesFor(int index);
}