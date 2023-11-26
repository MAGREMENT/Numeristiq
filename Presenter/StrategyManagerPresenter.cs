using Model;
using Presenter.Translator;

namespace Presenter;

public class StrategyManagerPresenter
{
    private readonly IStrategyLoader _loader;
    private readonly IStrategyManagerView _view;

    public StrategyManagerPresenter(IStrategyLoader loader, IStrategyManagerView view)
    {
        _loader = loader;
        _view = view;

        _loader.ListUpdated +=
            () => _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_loader.GetStrategyInfo()));
    }

    public void Start()
    {
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_loader.GetStrategyInfo()));
    }

    public void Search(string filter)
    {
        _view.ShowSearchResult(_loader.FindStrategies(filter));
    }

    public void AddStrategy(string s)
    {
        _loader.AddStrategy(s);
    }
    
    public void AddStrategy(string s, int position)
    {
        _loader.AddStrategy(s, position);
    }

    public void RemoveStrategy(int position)
    {
        _loader.RemoveStrategy(position);
    }

    public void InterchangeStrategies(int position1, int position2)
    {
        _loader.InterchangeStrategies(position1, position2);
    }
}