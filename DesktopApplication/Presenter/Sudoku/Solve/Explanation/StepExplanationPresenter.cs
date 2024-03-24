using Model.Helpers;
using Model.Sudoku.Solver.Explanation;

namespace DesktopApplication.Presenter.Sudoku.Solve.Explanation;

public class StepExplanationPresenter
{
    private readonly IStepExplanationView _view;
    private readonly ExplanationElement? _start;
    private readonly ISolvingState _state;

    public StepExplanationPresenter(IStepExplanationView view, ISolvingState state, ExplanationElement? start)
    {
        _view = view;
        _state = state;
        _start = start;
    }

    public void Initialize()
    {
        var drawer = _view.Drawer;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var number = _state[row, col];
                if (number == 0) drawer.ShowPossibilities(row, col, _state.PossibilitiesAt(row, col).EnumeratePossibilities());
                else drawer.ShowSolution(row, col, number);
            }
        }
        
        drawer.Refresh();
        _view.ShowExplanation(_start);
    }
}