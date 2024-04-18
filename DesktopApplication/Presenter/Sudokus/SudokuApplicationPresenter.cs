using System.Collections.Generic;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model;
using Model.Helpers;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Repository;

namespace DesktopApplication.Presenter.Sudokus;

public class SudokuApplicationPresenter : IStrategyRepositoryUpdater
{
    private readonly StrategyManager _strategyManager = new();
    private IRepository<IReadOnlyList<SudokuStrategy>>? _strategiesRepository;
    private readonly Settings _settings;

    public SudokuApplicationPresenter(Settings settings)
    {
        _settings = settings;
    }

    public SudokuSolvePresenter Initialize(ISudokuSolveView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        solver.ChangeBuffer = new StepManagingChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        
        return new SudokuSolvePresenter(view, solver, _settings, this);
    }

    public SudokuPlayPresenter Initialize(ISudokuPlayView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        solver.ChangeBuffer = new StepManagingChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        
        return new SudokuPlayPresenter(view, solver, _settings);
    }
    
    public SudokuManagePresenter Initialize(ISudokuManageView view)
    {
        return new SudokuManagePresenter(view, _strategyManager, this);
    }
    
    public SudokuGeneratePresenter Initialize(ISudokuGenerateView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        solver.ChangeBuffer = new FastChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        
        return new SudokuGeneratePresenter(view, solver, _settings);
    }

    public void InitializeApplication()
    {
        _strategiesRepository = new SudokuStrategiesJSONRepository("strategies.json");
        if (_strategiesRepository.Initialize(true))
        {
            _strategyManager.AddStrategies(_strategiesRepository.Download());
        }
        else _strategiesRepository = null;
    }

    public void Update()
    {
        _strategiesRepository?.Upload(_strategyManager.Strategies);
    }
}

public interface IStrategyRepositoryUpdater
{
    public void Update();
}