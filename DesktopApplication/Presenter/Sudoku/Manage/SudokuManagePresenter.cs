using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;
    private readonly StrategyManager _manager;

    public SudokuManagePresenter(ISudokuManageView view, StrategyManager manager)
    {
        _view = view;
        _manager = manager;
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
}