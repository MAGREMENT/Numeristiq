using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;
    private readonly StrategyManager _manager;
    private IStrategyRepositoryUpdater _updater;

    public SudokuManagePresenter(ISudokuManageView view, StrategyManager manager, 
        IStrategyRepositoryUpdater updater)
    {
        _view = view;
        _manager = manager;
        _updater = updater;
    }

    public void OnSearch(string s)
    {
        _view.ClearSearchResults();

        foreach (var result in StrategyPool.FindStrategies(s))
        {
            _view.AddSearchResult(result);
        }
    }

    public void InitStrategies()
    {
        _view.SetStrategyList(_manager.Strategies);
    }

    public void OnActiveStrategySelection(int index)
    {
        if (index < 0 || index >= _manager.Strategies.Count) return;

        var s = _manager.Strategies[index];
        _view.SetSelectedStrategyName(s.Name);
        _view.SetManageableSettings(new StrategySettingsPresenter(s, _updater));
    }
}