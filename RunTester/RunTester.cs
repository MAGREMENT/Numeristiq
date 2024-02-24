using System.Globalization;
using System.Text;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Trackers;
using Model.Utility;

namespace RunTester;

public class RunTester
{
    public string Path { get; set; } = "";
    public RunResult LastRunResult { get; private set; } = new();

    private bool _running;
    private SudokuSolver? _currentSolver;
    private RunResult? _currentRunResult;

    public delegate void OnSolveDone(int number, string line, bool success, bool solverFail);
    public event OnSolveDone? SolveDone;

    public delegate void OnRunStatusChanged(bool running);
    public event OnRunStatusChanged? RunStatusChanged;

    private readonly IRepository<IReadOnlyList<SudokuStrategy>> _repository;
    private readonly bool _toWaitForAll;

    public RunTester(IRepository<IReadOnlyList<SudokuStrategy>> repository, bool toWaitForAll)
    {
        _repository = repository;
        _repository.UploadAllowed = false;
        
        _toWaitForAll = toWaitForAll;
    }

    public void Start()
    {
        Run();
    }

    public void Cancel()
    {
        SetRunStatus(false);
    }

    private void Run()
    {
        SetRunStatus(true);
        
        _currentRunResult = new RunResult();

        _currentSolver = new SudokuSolver();
        _currentSolver.StrategyManager.AddStrategies(_repository.Download());
        if(_toWaitForAll) _currentSolver.StrategyManager.ChangeStrategyBehaviorForAll(OnCommitBehavior.WaitForAll);

        var tracker = new StatisticsTracker();
        _currentSolver.AddTracker(tracker);

        using TextReader reader = new StreamReader(Path, Encoding.UTF8);

        while (_running && reader.ReadLine() is { } line)
        {
            int commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];
            
            _currentSolver.SetSudoku(SudokuTranslator.TranslateLineFormat(s));
            _currentSolver.Solve();

            _currentRunResult.SolveDone(_currentSolver);
            SolveDone?.Invoke(_currentRunResult.Count, s, _currentSolver.Sudoku.IsCorrect(), _currentSolver.IsWrong());
        }

        _currentRunResult.RunFinished(tracker);
        LastRunResult = _currentRunResult;

        if(_running) SetRunStatus(false);
    }

    private void SetRunStatus(bool running)
    {
        _running = running;
        RunStatusChanged?.Invoke(running);
    }
}