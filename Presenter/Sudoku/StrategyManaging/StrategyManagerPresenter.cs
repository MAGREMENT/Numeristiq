using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Settings;
using Presenter.Sudoku.Translators;

namespace Presenter.Sudoku.StrategyManaging;

public class StrategyManagerPresenter
{
    private readonly StrategyManager _manager;
    private readonly IStrategyManagerView _view;

    public StrategyManagerPresenter(StrategyManager manager, IStrategyManagerView view)
    {
        _manager = manager;
        _view = view;
    }

    public void Start()
    {
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void Search(string filter)
    {
        _view.ShowSearchResult(StrategyPool.FindStrategies(filter));
    }

    public void AddStrategy(string s)
    {
        var strat = StrategyPool.CreateFrom(s);
        if (strat is null) return;
        
        _manager.AddStrategy(strat);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }
    
    public void AddStrategy(string s, int position)
    {
        var strat = StrategyPool.CreateFrom(s);
        if (strat is null) return;
        
        _manager.AddStrategy(strat, position);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void RemoveStrategy(int position)
    {
        _manager.RemoveStrategy(position);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void InterchangeStrategies(int position1, int position2)
    {
        _manager.InterchangeStrategies(position1, position2);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void ShowStrategy(int position)
    {
        var info = _manager.Strategies;
        if(position < info.Count) _view.ShowStrategy(ModelToViewTranslator.Translate(info[position]));
    }

    public void ChangeStrategyBehavior(string name, OnCommitBehavior behavior)
    {
        _manager.ChangeStrategyBehavior(name, behavior);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void ChangeStrategyUsage(string name, bool yes)
    {
        _manager.ChangeStrategyUsage(name, yes);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }

    public void ChangeArgument(string strategyName, string argumentName, SettingValue value)
    {
        _manager.ChangeArgument(strategyName, argumentName, value);
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.Strategies));
    }
}