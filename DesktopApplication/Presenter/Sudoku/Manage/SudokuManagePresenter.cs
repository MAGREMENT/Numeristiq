using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;
    private readonly StrategyManager _manager;
    private readonly IStrategyRepositoryUpdater _updater;

    public SudokuManagePresenter(ISudokuManageView view, StrategyManager manager, 
        IStrategyRepositoryUpdater updater)
    {
        _view = view;
        _manager = manager;
        _updater = updater;
    }
    
    public void Initialize()
    {
        _view.SetStrategyList(_manager.Strategies);

        foreach (var result in StrategyPool.EnumerateStrategies())
        {
            _view.AddSearchResult(result);
        }
    }

    public void OnSearch(string s)
    {
        _view.ClearSearchResults();

        foreach (var result in StrategyPool.EnumerateStrategies(s))
        {
            _view.AddSearchResult(result);
        }
    }
    
    public void OnActiveStrategySelection(int index)
    {
        if (index < 0 || index >= _manager.Strategies.Count) return;

        var s = _manager.Strategies[index];
        _view.SetSelectedStrategyName(s.Name);
        _view.SetManageableSettings(new StrategySettingsPresenter(s, _updater));
    }

    public void AddStrategy(string s, int position)
    {
        var strategy = StrategyPool.CreateFrom(s);
        if (strategy is null) return;
        
        _manager.AddStrategy(strategy, position);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void InterchangeStrategies(int posFrom, int posTo)
    {
        _manager.InterchangeStrategies(posFrom, posTo);
        _view.SetStrategyList(_manager.Strategies);
    }

    public void RemoveStrategy(int index)
    {
        _manager.RemoveStrategy(index);
        _view.SetStrategyList(_manager.Strategies);
        _view.ClearSelectedStrategy();
    }

    public void OnShow()
    {
        _view.ClearSelectedStrategy();
    }
}