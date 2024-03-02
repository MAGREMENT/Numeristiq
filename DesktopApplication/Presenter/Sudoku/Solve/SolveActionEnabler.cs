namespace DesktopApplication.Presenter.Sudoku.Solve;

public class SolveActionEnabler
{
    private readonly ISudokuSolveView _view;

    private ulong _currentlyDisabling;

    public SolveActionEnabler(ISudokuSolveView view)
    {
        _view = view;
    }

    public void EnableActions(int id)
    {
        _currentlyDisabling &= ~(1ul << id);
        if(System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _view.EnableSolveActions();
    }

    public void DisableActions(int id)
    {
        if(System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _view.DisableSolveActions();
        _currentlyDisabling |= 1ul << id;
    }
}