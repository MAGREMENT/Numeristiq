namespace Presenter.Sudoku.Solver;

public class SolverActionEnabler
{
    private readonly ISolverView _view;

    private ulong _currentlyDisabling;

    public SolverActionEnabler(ISolverView view)
    {
        _view = view;
    }

    public void EnableActions(int id)
    {
        _currentlyDisabling &= ~(1ul << id);
        if(System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _view.EnableActions();
    }

    public void DisableActions(int id)
    {
        if(System.Numerics.BitOperations.PopCount(_currentlyDisabling) == 0) _view.DisableActions();
        _currentlyDisabling |= 1ul << id;
    }
}