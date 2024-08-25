using Model.YourPuzzles;

namespace DesktopApplication.Presenter.YourPuzzles;

public class YourPuzzlePresenter
{
    private readonly NumericYourPuzzle _puzzle = new(0, 0);
    private readonly IYourPuzzleView _view;

    public YourPuzzlePresenter(IYourPuzzleView view)
    {
        _view = view;
    }

    public void SetRowCount(int diff)
    {
        _puzzle.ChangeSize(_puzzle.RowCount + diff, _puzzle.ColumnCount);
        ShowPuzzle();
    }

    public void SetColumnCount(int diff)
    {
        _puzzle.ChangeSize(_puzzle.RowCount, _puzzle.ColumnCount + diff);
        ShowPuzzle();
    }

    private void ShowPuzzle()
    {
        var drawer = _view.Drawer;

        drawer.RowCount = _puzzle.RowCount;
        drawer.ColumnCount = _puzzle.ColumnCount;
        
        drawer.Refresh();
    }
}