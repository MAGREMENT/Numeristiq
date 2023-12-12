using Global;

namespace Presenter.Player;

public class PlayerPresenter
{
    private readonly IPlayerView _view;
    private HashSet<Cell> _selected = new();

    public PlayerPresenter(IPlayerView view)
    {
        _view = view;
    }

    public void RestartSelection(int rowFrom, int colFrom)
    {
        var cell = new Cell(rowFrom, colFrom);
        var contained = _selected.Contains(cell);
        
        _selected.Clear();
        _view.ClearCursor();

        if (!contained)
        {
            _selected.Add(cell);
            _view.PutCursorOn(_selected);
        }
        
        _view.Refresh();
    }

    public void AddToSelection(int row, int col)
    {
        var count = _selected.Count;
        _selected.Add(new Cell(row, col));
        
        if (_selected.Count > count)
        {
            _view.PutCursorOn(_selected);
            _view.Refresh();
        }
    }
}