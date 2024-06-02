using Model.Core;
using Model.Helpers;
using Model.Helpers.Highlighting;

namespace Model.Kakuros;

public interface IKakuroStrategyUser : ISolvingState
{
    IReadOnlyKakuro Kakuro { get; }
    ChangeBuffer<IUpdatableSolvingState, ISolvingStateHighlighter> ChangeBuffer { get; }
}