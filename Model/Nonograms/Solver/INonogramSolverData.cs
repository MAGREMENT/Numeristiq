using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Nonograms.Solver;

public interface INonogramSolverData
{
    DichotomousChangeBuffer<INonogramSolvingState, INonogramHighlighter> ChangeBuffer { get; }
    IReadOnlyNonogram Nonogram { get; }
    bool IsAvailable(int row, int col);
    NonogramPreComputer PreComputer { get; }
}