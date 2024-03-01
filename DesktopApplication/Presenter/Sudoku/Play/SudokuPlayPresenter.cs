namespace DesktopApplication.Presenter.Sudoku.Play;

public class SudokuPlayPresenter
{
    private readonly ISudokuPlayView _view;

    public SudokuPlayPresenter(ISudokuPlayView view)
    {
        _view = view;
    }
}