namespace DesktopApplication.Presenter.Sudoku.Generate;

public class SudokuGeneratePresenter
{
    private readonly ISudokuGenerateView _view;

    public SudokuGeneratePresenter(ISudokuGenerateView view)
    {
        _view = view;
    }
}