using System.Collections.Generic;
using DesktopApplication.Presenter.Sudoku.Generate;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.Presenter.Sudoku.Play;
using DesktopApplication.Presenter.Sudoku.Solve;
using Model;
using Model.Helpers;
using Model.Helpers.Changes.Buffers;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Repository;

namespace DesktopApplication.Presenter.Sudoku;

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
        solver.ChangeBuffer = new LogManagedChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        
        return new SudokuSolvePresenter(view, solver, _settings, this);
    }

    public SudokuPlayPresenter Initialize(ISudokuPlayView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        solver.ChangeBuffer = new LogManagedChangeBuffer<IUpdatableSudokuSolvingState, ISudokuHighlighter>(solver);
        
        return new SudokuPlayPresenter(view, solver, _settings);
    }
    
    public SudokuManagePresenter Initialize(ISudokuManageView view)
    {
        return new SudokuManagePresenter(view, _strategyManager, this);
    }
    
    public SudokuGeneratePresenter Initialize(ISudokuGenerateView view)
    {
        return new SudokuGeneratePresenter(view);
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