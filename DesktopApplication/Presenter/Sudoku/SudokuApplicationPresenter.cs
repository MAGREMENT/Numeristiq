using System.Collections.Generic;
using DesktopApplication.Presenter.Sudoku.Generate;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.Presenter.Sudoku.Play;
using DesktopApplication.Presenter.Sudoku.Solve;
using Model;
using Model.Helpers;
using Model.Helpers.Changes.Buffers;
using Model.Sudoku.Solver;
using Repository;

namespace DesktopApplication.Presenter.Sudoku;

public class SudokuApplicationPresenter
{
    private readonly SudokuSolver _solver = new();
    private IRepository<IReadOnlyList<SudokuStrategy>>? _strategiesRepository;
    
    public SudokuSolvePresenter Initialize(ISudokuSolveView view)
    {
        return new SudokuSolvePresenter(view, _solver);
    }

    public SudokuPlayPresenter Initialize(ISudokuPlayView view)
    {
        return new SudokuPlayPresenter(view);
    }
    
    public SudokuManagePresenter Initialize(ISudokuManageView view)
    {
        return new SudokuManagePresenter(view);
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
            _solver.StrategyManager.AddStrategies(_strategiesRepository.Download());
        }
        else _strategiesRepository = null;

        _solver.ChangeBuffer = new LogManagedChangeBuffer<IUpdatableSudokuSolvingState>(_solver);
    }
}