using System.IO;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model.Repositories;
using Model.Sudokus.Solver;
using Repository;

namespace DesktopApplication.Presenter.Sudokus;

public class SudokuApplicationPresenter
{
    private readonly StrategyManager<SudokuStrategy> _strategyManager = new();
    private readonly IStrategyRepository<SudokuStrategy> _strategiesRepository;
    private readonly Settings _settings;

    public SudokuApplicationPresenter(Settings settings)
    {
        _settings = settings;
        _strategiesRepository = new SudokuStrategyJsonRepository("strategies.json", 
            !GlobalApplicationPresenter.IsForProduction, true);
        _strategyManager.AddStrategies(_strategiesRepository.GetStrategies());
    }

    public SudokuSolvePresenter Initialize(ISudokuSolveView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        
        return new SudokuSolvePresenter(view, solver, _settings, _strategiesRepository);
    }

    public SudokuPlayPresenter Initialize(ISudokuPlayView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        
        return new SudokuPlayPresenter(view, solver, _settings);
    }
    
    public SudokuManagePresenter Initialize(ISudokuManageView view)
    {
        return new SudokuManagePresenter(view, _strategyManager, _strategiesRepository);
    }
    
    public SudokuGeneratePresenter Initialize(ISudokuGenerateView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        
        return new SudokuGeneratePresenter(view, solver, _settings);
    }
}