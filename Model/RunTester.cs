using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Model.Solver;
using Model.Solver.Helpers;

namespace Model;

public class RunTester
{
    public string Path { get; set; } = "";
    public RunResult LastRunResult { get; private set; } = new();

    private bool _running;
    private Solver.Solver? _currentSolver;
    private RunResult? _currentRunResult;

    public delegate void OnSolveDone(int number, string line, bool success, bool solverFail);
    public event OnSolveDone? SolveDone;

    public delegate void OnRunStatusChanged(bool running);
    public event OnRunStatusChanged? RunStatusChanged;

    public async void Start()
    {
        await Task.Run(Run);
    }

    public void Cancel()
    {
        SetRunStatus(false);
    }

    private void Run()
    {
        SetRunStatus(true);
        
        _currentRunResult = new RunResult();
        
        _currentSolver = new Solver.Solver(new Sudoku())
        {
            LogsManaged = false,
            StatisticsTracked = true
        };

        using TextReader reader = new StreamReader(Path, Encoding.UTF8);

        while (_running && reader.ReadLine() is { } line)
        {
            _currentSolver.SetSudoku(SudokuTranslator.Translate(line));
            _currentSolver.Solve();

            _currentRunResult.SolveDone(_currentSolver);
            SolveDone?.Invoke(_currentRunResult.Count, line, _currentSolver.Sudoku.IsCorrect(), _currentSolver.IsWrong());
        }

        _currentRunResult.RunFinished(_currentSolver);
        LastRunResult = _currentRunResult;

        if(_running) SetRunStatus(false);
    }

    private void SetRunStatus(bool running)
    {
        _running = running;
        RunStatusChanged?.Invoke(running);
    }
}

public class RunResult
{
    public int Count { get; private set; }
    public int Success { get; private set; }
    public int SolverFails { get; private set; }
    public IReadOnlyList<StrategyReport> Reports => _reports;
    
    private readonly List<StrategyReport> _reports = new();

    public void SolveDone(Solver.Solver solver)
    {
        Count++;
        if (solver.Sudoku.IsCorrect()) Success++;
        else if (solver.IsWrong()) SolverFails++;
    }

    public void RunFinished(Solver.Solver solver)
    {
        foreach (var strategy in solver.Strategies)
        {
            _reports.Add(new StrategyReport(strategy));
        }
    }
}

public class StrategyReport
{
    public string StrategyName { get; }
    public StrategyDifficulty Difficulty { get; }
    public IReadOnlyTracker Tracker { get; }

    public StrategyReport(IStrategy strategy)
    {
        StrategyName = strategy.Name;
        Difficulty = strategy.Difficulty;
        Tracker = strategy.Tracker;
    }
}