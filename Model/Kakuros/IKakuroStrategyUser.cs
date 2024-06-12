using Model.Core;
using Model.Core.Changes;
using Model.Core.Highlighting;

namespace Model.Kakuros;

public interface IKakuroStrategyUser : ISolvingState
{
    IReadOnlyKakuro Kakuro { get; }
    ChangeBuffer<IUpdatableSolvingState, ISolvingStateHighlighter> ChangeBuffer { get; }
}