using Presenter.Sudoku.Translators;

namespace Presenter.Sudoku.StrategyManaging;

public interface IStrategyManagerView
{
    void ShowSearchResult(List<string> result);
    void SetStrategiesUsed(IReadOnlyList<ViewStrategy> strategies);
    void ShowStrategy(ViewStrategy strategy);
}