using Model.Sudoku.Solver;

namespace DesktopApplication.Presenter.Sudoku.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;

    public SudokuManagePresenter(ISudokuManageView view)
    {
        _view = view;
    }

    public void OnSearch(string s)
    {
        _view.ClearSearchResults();

        foreach (var result in StrategyPool.FindStrategies(s))
        {
            _view.AddSearchResult(result);
        }
    }
}