using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Kakuros;

public interface IKakuroStrategyUser : ISolvingState
{
    IReadOnlyKakuro Kakuro { get; }
    IChangeBuffer<IUpdatableSolvingState, ISolvingStateHighlighter> ChangeBuffer { get; set; }
}