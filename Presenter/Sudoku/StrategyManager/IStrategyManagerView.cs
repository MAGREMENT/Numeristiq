using Presenter.Sudoku.Translators;

namespace Presenter.Sudoku.StrategyManager;

public interface IStrategyManagerView
{
    void ShowSearchResult(List<string> result);
    void SetStrategiesUsed(IReadOnlyList<ViewStrategy> strategies);
    void ShowStrategy(ViewStrategy strategy);
}