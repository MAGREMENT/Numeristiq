using System.Collections.Generic;
using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Solver;

public class NonogramSolvingState : IUpdatableDichotomousSolvingState //TODO
{
    public bool this[int row, int col] => true;

    public bool IsAvailable(int row, int col)
    {
        return false;
    }

    public IUpdatableDichotomousSolvingState Apply(IEnumerable<DichotomousChange> progresses)
    {
        return this;
    }

    public IUpdatableDichotomousSolvingState Apply(DichotomousChange progress)
    {
        return this;
    }
}