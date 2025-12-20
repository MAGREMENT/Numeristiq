using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;
using Model.Sudokus.Solver.Highlighting;

namespace Model.Kakuros;

public interface IKakuroSolverData : INumericSolvingState
{
    IReadOnlyKakuro Kakuro { get; }
    NumericChangeBuffer<INumericSolvingState, INumericSolvingStateHighlighter> ChangeBuffer { get; }
}