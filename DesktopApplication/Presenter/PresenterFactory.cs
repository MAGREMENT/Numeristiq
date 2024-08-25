using System.Collections.Generic;
using DesktopApplication.Presenter.Binairos.Solve;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.Presenter.Nonograms.Solve;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.Presenter.Themes;
using DesktopApplication.Presenter.YourPuzzles;
using Model.Binairos;
using Model.Core;
using Model.Kakuros;
using Model.Nonograms.Solver;
using Model.Repositories;
using Model.Sudokus.Solver;
using Model.Tectonics.Solver;
using Repository;
using Repository.Files;
using Repository.Files.Types;
using Repository.HardCoded;

namespace DesktopApplication.Presenter;

public class PresenterFactory
{
    public const bool IsForProduction = false;
    
    private readonly Settings _settings;
    private readonly ThemeManager _themeManager;
    
    private readonly IStrategyRepository<SudokuStrategy> _sudokuRepository;
    
    private readonly StrategyManager<SudokuStrategy> _sudokuManager = new();
    private readonly StrategyManager<Strategy<ITectonicSolverData>> _tectonicManager = new();
    private readonly StrategyManager<Strategy<IKakuroSolverData>> _kakuroManager = new();
    private readonly StrategyManager<Strategy<INonogramSolverData>> _nonogramManager = new();
    private readonly StrategyManager<Strategy<IBinairoSolverData>> _binairoManager = new();

    private PresenterFactory(ISettingRepository settingsRepository, ThemeMultiRepository repository,
        IStrategyRepository<SudokuStrategy> sudokuRepository, 
        IStrategyRepository<Strategy<ITectonicSolverData>> tectonicRepository,
        IStrategyRepository<Strategy<IKakuroSolverData>> kakuroRepository,
        IStrategyRepository<Strategy<INonogramSolverData>> nonogramRepository,
        IStrategyRepository<Strategy<IBinairoSolverData>> binairoRepository)
    {
        _themeManager = new ThemeManager(repository);
        _settings = new Settings(_themeManager.Themes, settingsRepository);
        _sudokuRepository = sudokuRepository;
        
        _sudokuManager.AddStrategies(sudokuRepository.GetStrategies());
        _tectonicManager.AddStrategies(tectonicRepository.GetStrategies());
        _kakuroManager.AddStrategies(kakuroRepository.GetStrategies());
        _nonogramManager.AddStrategies(nonogramRepository.GetStrategies());
        _binairoManager.AddStrategies(binairoRepository.GetStrategies());
        
        foreach (var entry in settingsRepository.GetSettings())
        {
            _settings.TrySet(entry.Key, entry.Value, false, false);
        }
    }

    public ResourcePresenter Initialize(IResourceView view) => new(_settings, _themeManager, view);

    public WelcomePresenter Initialize() => new(_settings);

    public ThemePresenter Initialize(IThemeView view) => new(_themeManager, _settings, view);

    public TectonicSolvePresenter Initialize(ITectonicSolveView view) => new(new TectonicSolver 
    {
        StrategyManager = _tectonicManager
    }, view, _settings);

    public SudokuSolvePresenter Initialize(ISudokuSolveView view) => new(view, new SudokuSolver
    {
        StrategyManager = _sudokuManager
    }, _settings, _sudokuRepository);

    public SudokuPlayPresenter Initialize(ISudokuPlayView view) => new(view, new SudokuSolver
    {
        StrategyManager = _sudokuManager
    }, _settings);

    public SudokuManagePresenter Initialize(ISudokuManageView view) => new(view, _sudokuManager, _sudokuRepository);

    public SudokuGeneratePresenter Initialize(ISudokuGenerateView view) => new(view, new SudokuSolver
    {
        StrategyManager = _sudokuManager
    }, _settings);

    public KakuroSolvePresenter Initialize(IKakuroSolveView view) => new(view, 
        new KakuroSolver(new RecursiveKakuroCombinationCalculator())
    {
        StrategyManager = _kakuroManager
    }, _settings);

    public NonogramSolvePresenter Initialize(INonogramSolveView view) => new(view, new NonogramSolver
    {
        StrategyManager = _nonogramManager
    });

    public BinairoSolvePresenter Initialize(IBinairoSolveView view) => new(view, new BinairoSolver
    {
        StrategyManager = _binairoManager
    }, _settings);

    public YourPuzzlePresenter Initialize(IYourPuzzleView view) => new(view);
    
    #region Instance

    private static PresenterFactory? _instance;

    public static PresenterFactory Instance
    {
        get
        {
            _instance ??= InitializeInstance();
            return _instance;
        }
    }
    
    private static PresenterFactory InitializeInstance()
    {
        var themeRepository = new ThemeMultiRepository(new FileThemeRepository("themes",
                !IsForProduction, true, new ThemeNativeType()),
            new HardCodedThemeRepository());
        var settingsRepository = new FileSettingsRepository("settings", 
            !IsForProduction, true, new JsonType<Dictionary<string, string>>());
        var sudokuRepository = new FileSudokuStrategiesRepository("strategies", 
            !IsForProduction, true, new JsonType<List<StrategyDAO>>());
        var tectonicRepository = new HardCodedTectonicStrategyRepository();
        var kakuroRepository = new HardCodedKakuroStrategyRepository();
        var nonogramRepository = new HardCodedNonogramStrategyRepository();
        var binairoRepository = new HardCodedBinairoStrategyRepository();

        return new PresenterFactory(settingsRepository, themeRepository, sudokuRepository, tectonicRepository,
            kakuroRepository, nonogramRepository, binairoRepository);
    }

    #endregion
}