using Model;
using Model.Player;
using Model.Solver;
using Model.Solver.Helpers.Changes;
using Presenter.Player;
using Presenter.Solver;
using Presenter.StepChooser;
using Presenter.StrategyManager;
using Repository;

namespace Presenter;

public class PresenterFactory
{
    private readonly ISolver _solver;
    private readonly IPlayer _player;

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

        _player = new SudokuPlayer();
    }

    public SolverPresenter Create(ISolverView view)
    {
        return new SolverPresenter(_solver, view);
    }

    public StrategyManagerPresenter Create(IStrategyManagerView view)
    {
        return new StrategyManagerPresenter(_solver.StrategyLoader, view);
    }

    public PlayerPresenter Create(IPlayerView view)
    {
        return new PlayerPresenter(_player, view);
    }
}

public class StepChooserPresenterBuilder
{
    private readonly SolverState _state;
    private readonly BuiltChangeCommit[] _commits;
    private readonly IStepChooserCallback _callback;

    public StepChooserPresenterBuilder(SolverState state, BuiltChangeCommit[] commits, IStepChooserCallback callback)
    {
        _commits = commits;
        _callback = callback;
        _state = state;
    }

    public StepChooserPresenter Build(IStepChooserView view)
    {
        return new StepChooserPresenter(_state, _commits, view, _callback);
    }
}