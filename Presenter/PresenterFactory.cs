using Model;
using Model.Solver;

namespace Presenter;

public class PresenterFactory
{
    private readonly ISolver _solver = new SudokuSolver();

    public SolverPresenter Create(ISolverView view)
    {
        return new SolverPresenter(_solver, view);
    }

    public StrategyManagerPresenter Create(IStrategyManagerView view)
    {
        return new StrategyManagerPresenter(_solver.StrategyLoader, view);
    }
}