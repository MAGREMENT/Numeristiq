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

public class ApplicationPresenter
{
    private readonly IViewManager _manager;
    
    private readonly Settings _settings;
    private readonly ISolver _solver;
    private readonly IPlayer _player;
    
    

    private ApplicationPresenter(IViewManager manager)
    {
        _manager = manager;
        
        var solver = new SudokuSolver
        {
            StatisticsTracked = false,
            LogsManaged = true
        };
        var strategyRepository = new JSONRepository<List<StrategyDAO>>("strategies.json");
        try
        {
            strategyRepository.Initialize();
        }
        catch (RepositoryInitializationException)
        {
            strategyRepository.New(new List<StrategyDAO>());
        }

        solver.Bind(strategyRepository);
        _solver = solver;

        _player = new SudokuPlayer();

        _settings = new Settings();
        var settingsRepository = new JSONRepository<SettingsDAO>("settings.json");
        try
        {
            settingsRepository.Initialize();
        }
        catch (RepositoryInitializationException)
        {
            settingsRepository.New(_settings.ToDAO());
        }

        _settings.Bind(settingsRepository);
    }

    public static ApplicationPresenter Initialize(IViewManager manager)
    {
        return new ApplicationPresenter(manager);
    }

    public SolverPresenter Create(ISolverView view)
    {
        return new SolverPresenter(_solver, view, _settings);
    }

    public StrategyManagerPresenter Create(IStrategyManagerView view)
    {
        return new StrategyManagerPresenter(_solver.StrategyLoader, view);
    }

    public PlayerPresenter Create(IPlayerView view)
    {
        return new PlayerPresenter(_player, view, _settings);
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

public record ThemeDAO(RGB Background1, RGB Background2, RGB Background3, RGB Primary1, RGB Primary2,
    RGB Secondary1, RGB Secondary2, RGB Accent, RGB Text);

public readonly struct RGB
{
    public RGB(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public byte Red { get; }
    public byte Green { get; }
    public byte Blue { get; }
}