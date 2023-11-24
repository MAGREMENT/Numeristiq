using Model;

namespace Presenter;

public class StrategyManagerPresenter
{
    private readonly IStrategyLoader _loader;
    private readonly IStrategyManagerView _view;

    public StrategyManagerPresenter(IStrategyLoader loader, IStrategyManagerView view)
    {
        _loader = loader;
        _view = view;
    }

    public void Search(string filter)
    {
        _view.ShowSearchResult(_loader.FindStrategies(filter));
    }
}