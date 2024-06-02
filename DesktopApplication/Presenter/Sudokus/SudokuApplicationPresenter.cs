using System.Collections.Generic;
using System.IO;
using DesktopApplication.Presenter.Sudokus.Generate;
using DesktopApplication.Presenter.Sudokus.Manage;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using Model;
using Model.Sudokus.Solver;
using Repository;

namespace DesktopApplication.Presenter.Sudokus;

public class SudokuApplicationPresenter : IStrategyRepositoryUpdater
{
    private readonly StrategyManager<SudokuStrategy> _strategyManager = new();
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
        
        return new SudokuSolvePresenter(view, solver, _settings, this);
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
        return new SudokuManagePresenter(view, _strategyManager, this);
    }
    
    public SudokuGeneratePresenter Initialize(ISudokuGenerateView view)
    {
        var solver = new SudokuSolver
        {
            StrategyManager = _strategyManager
        };
        
        return new SudokuGeneratePresenter(view, solver, _settings);
    }

    public void InitializeApplication()
    {
        _strategiesRepository = new SudokuStrategiesJSONRepository(
            GlobalApplicationPresenter.PathInstantiator.Instantiate("strategies.json"));
        _strategyManager.AddStrategies(_strategiesRepository.Download());
    }

    public void Update()
    {
        _strategiesRepository?.Upload(_strategyManager.Strategies);
    }

    public void Upload(Stream stream)
    {
        if (_strategiesRepository is not SudokuStrategiesJSONRepository r) return;
        r.Upload(_strategyManager.Strategies, stream);
    }

    public void Download(Stream stream)
    {
        if (_strategiesRepository is not SudokuStrategiesJSONRepository r) return;
        _strategyManager.ClearStrategies();
        _strategyManager.AddStrategies(r.Download(stream));
    }
}

public interface IStrategyRepositoryUpdater
{
    public void Update();
    public void Upload(Stream stream);
    public void Download(Stream stream);
}