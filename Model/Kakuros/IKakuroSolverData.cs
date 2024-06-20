using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Kakuros;

public interface IKakuroSolverData : ISolvingState
{
    IReadOnlyKakuro Kakuro { get; }
    ChangeBuffer<IUpdatableSolvingState, ISolvingStateHighlighter> ChangeBuffer { get; }
}