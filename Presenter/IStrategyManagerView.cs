using Presenter.Translator;

namespace Presenter;

public interface IStrategyManagerView
{
    void ShowSearchResult(List<string> result);
    void SetStrategiesUsed(IReadOnlyList<ViewStrategy> strategies);
}