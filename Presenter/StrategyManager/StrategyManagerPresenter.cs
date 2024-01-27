using Model;
using Model.SudokuSolving;
using Model.SudokuSolving.Solver;
using Presenter.Translators;

namespace Presenter.StrategyManager;

public class StrategyManagerPresenter
{
    private readonly IStrategyLoader _loader;
    private readonly IStrategyManagerView _view;

    public StrategyManagerPresenter(IStrategyLoader loader, IStrategyManagerView view)
    {
        _loader = loader;
        _view = view;

        _loader.ListUpdated +=
            () => _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_loader.GetStrategiesInformation()));
    }

    public void Start()
    {
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_loader.GetStrategiesInformation()));
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

    public void ShowStrategy(int position)
    {
        var info = _loader.GetStrategiesInformation();
        if(position < info.Length) _view.ShowStrategy(ModelToViewTranslator.Translate(info[position]));
    }

    public void ChangeStrategyBehavior(string name, OnCommitBehavior behavior)
    {
        _loader.ChangeStrategyBehavior(name, behavior);
    }

    public void ChangeStrategyUsage(string name, bool yes)
    {
        _loader.ChangeStrategyUsage(name, yes);
    }

    public void ChangeArgument(string strategyName, string argumentName, string value)
    {
        _loader.ChangeArgument(strategyName, argumentName, value);
    }
}