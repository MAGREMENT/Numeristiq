using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Settings;
using Presenter.Sudoku.Translators;

namespace Presenter.Sudoku.StrategyManager;

public class StrategyManagerPresenter
{
    private readonly IStrategyManager _manager;
    private readonly IStrategyManagerView _view;

    public StrategyManagerPresenter(IStrategyManager manager, IStrategyManagerView view)
    {
        _manager = manager;
        _view = view;

        _manager.ListUpdated +=
            () => _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.GetStrategiesInformation()));
    }

    public void Start()
    {
        _view.SetStrategiesUsed(ModelToViewTranslator.Translate(_manager.GetStrategiesInformation()));
    }

    public void Search(string filter)
    {
        _view.ShowSearchResult(_manager.FindStrategies(filter));
    }

    public void AddStrategy(string s)
    {
        _manager.AddStrategy(s);
    }
    
    public void AddStrategy(string s, int position)
    {
        _manager.AddStrategy(s, position);
    }

    public void RemoveStrategy(int position)
    {
        _manager.RemoveStrategy(position);
    }

    public void InterchangeStrategies(int position1, int position2)
    {
        _manager.InterchangeStrategies(position1, position2);
    }

    public void ShowStrategy(int position)
    {
        var info = _manager.GetStrategiesInformation();
        if(position < info.Length) _view.ShowStrategy(ModelToViewTranslator.Translate(info[position]));
    }

    public void ChangeStrategyBehavior(string name, OnCommitBehavior behavior)
    {
        _manager.ChangeStrategyBehavior(name, behavior);
    }

    public void ChangeStrategyUsage(string name, bool yes)
    {
        _manager.ChangeStrategyUsage(name, yes);
    }

    public void ChangeArgument(string strategyName, string argumentName, SettingValue value)
    {
        _manager.ChangeArgument(strategyName, argumentName, value);
    }
}