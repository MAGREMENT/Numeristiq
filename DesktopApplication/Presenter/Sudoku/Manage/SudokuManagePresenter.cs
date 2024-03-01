namespace DesktopApplication.Presenter.Sudoku.Manage;

public class SudokuManagePresenter
{
    private readonly ISudokuManageView _view;

    public SudokuManagePresenter(ISudokuManageView view)
    {
        _view = view;
    }
}