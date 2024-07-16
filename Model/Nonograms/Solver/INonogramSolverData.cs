using Model.Core;
using Model.Core.BackTracking;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Nonograms.Solver;

public interface INonogramSolverData : IAvailabilityChecker
{
    DichotomousChangeBuffer<INonogramSolvingState, INonogramHighlighter> ChangeBuffer { get; }
    IReadOnlyNonogram Nonogram { get; }
    NonogramPreComputer PreComputer { get; }
}