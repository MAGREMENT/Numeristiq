using Model.Core;
using Model.Core.Changes;

namespace Model.Nonograms.Solver;

public interface INonogramSolverData
{
    DichotomousChangeBuffer<IUpdatableDichotomousSolvingState, object> ChangeBuffer { get; }
    IReadOnlyNonogram Nonogram { get; }
    bool IsAvailable(int row, int col);
    NonogramPreComputer PreComputer { get; }
}