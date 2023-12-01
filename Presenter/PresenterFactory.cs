using Model;
using Model.Solver;
using Presenter.Solver;
using Presenter.StrategyManager;
using Repository;

namespace Presenter;

public class PresenterFactory
{
    private readonly ISolver _solver;

    public PresenterFactory()
    {
        var repository = new JSONRepository<List<StrategyDAO>>("strategies.json");
        try
        {
            repository.Initialize();
        }
        catch (RepositoryInitializationException)
        {
            repository.New(new List<StrategyDAO>());
        }
        
        
        _solver = new SudokuSolver(repository)
        {
            StatisticsTracked = false,
            LogsManaged = true
        };
    }

    public SolverPresenter Create(ISolverView view)
    {
        return new SolverPresenter(_solver, view);
    }

    public StrategyManagerPresenter Create(IStrategyManagerView view)
    {
        return new StrategyManagerPresenter(_solver.StrategyLoader, view);
    }
}