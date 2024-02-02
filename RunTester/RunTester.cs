using System.Globalization;
using System.Text;
using Model;
using Model.Sudoku;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Helpers;
using Model.Sudoku.Utility;

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

    private readonly IRepository<List<StrategyDAO>> _repository;
    private readonly bool _toWaitForAll;

    public RunTester(IRepository<List<StrategyDAO>> repository, bool toWaitForAll)
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
        
        _currentSolver = new SudokuSolver
        {
            LogsManaged = false,
            StatisticsTracked = true
        };
        _currentSolver.Bind(_repository);
        if(_toWaitForAll) _currentSolver.StrategyManager.ChangeStrategyBehaviorForAll(OnCommitBehavior.WaitForAll);

        using TextReader reader = new StreamReader(Path, Encoding.UTF8);

        while (_running && reader.ReadLine() is { } line)
        {
            int commentStart = line.IndexOf('#');
            var s = commentStart == -1 ? line : line[..commentStart];
            
            _currentSolver.SetSudoku(SudokuTranslator.TranslateToSudoku(s));
            _currentSolver.Solve();

            _currentRunResult.SolveDone(_currentSolver);
            SolveDone?.Invoke(_currentRunResult.Count, s, _currentSolver.Sudoku.IsCorrect(), _currentSolver.IsWrong());
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

    public void SolveDone(SudokuSolver solver)
    {
        Count++;
        if (solver.Sudoku.IsCorrect()) Success++;
        else if (solver.IsWrong()) SolverFails++;
    }

    public void RunFinished(SudokuSolver solver)
    {
        foreach (var strategy in solver.GetStrategyInfo())
        {
            _reports.Add(new StrategyReport(strategy));
        }
    }

    public override string ToString()
    {
        string[] columnTitles = { "Strategy name", "Usage", "Score", "Solutions added", "Possibilities removed",
            "Score percentage", "Total time", "Average time" };
        int[] widthCap = new int[columnTitles.Length];

        for (int i = 0; i < columnTitles.Length; i++)
        {
            widthCap[i] = columnTitles[i].Length + 2;
        }

        foreach (var report in Reports)
        {
            widthCap[0] = Math.Max(widthCap[0], report.StrategyName.Length + 2);
            widthCap[1] = Math.Max(widthCap[1], report.Tracker.Usage.ToString().Length + 2);
            widthCap[2] = Math.Max(widthCap[2], report.Tracker.Score.ToString().Length + 2);
            widthCap[3] = Math.Max(widthCap[3], report.Tracker.SolutionsAdded.ToString().Length + 2);
            widthCap[4] = Math.Max(widthCap[4], report.Tracker.PossibilitiesRemoved.ToString().Length + 2);
            widthCap[5] = Math.Max(widthCap[5], report.Tracker.ScorePercentage()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[6] = Math.Max(widthCap[6], report.Tracker.TotalTimeInSecond()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[7] = Math.Max(widthCap[7], report.Tracker.AverageTime()
                .ToString(CultureInfo.InvariantCulture).Length + 4);
        }

        var totalWidth = columnTitles.Length - 1;

        foreach (var width in widthCap)
        {
            totalWidth += width;
        }

        var result = new StringBuilder(StringUtility.FillEvenlyWith("Result", '-', totalWidth) + "\n\n");

        result.Append($"Completion rate : {Success} / {Count}\n");
        result.Append($"Solver fails : {SolverFails}\n\n");

        result.Append(StringUtility.FillEvenlyWith(columnTitles[0], ' ', widthCap[0]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[1], ' ', widthCap[1]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[2], ' ', widthCap[2]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[3], ' ', widthCap[3]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[4], ' ', widthCap[4]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[5], ' ', widthCap[5]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[6], ' ', widthCap[6]) + "|"
            + StringUtility.FillEvenlyWith(columnTitles[7], ' ', widthCap[7]) + "\n");
        result.Append(CrossRow(widthCap));
        
        var totalStrategyTime = 0L;

        foreach (var report in Reports)
        {
            result.Append(StringUtility.FillEvenlyWith(report.StrategyName, ' ', widthCap[0]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.Usage.ToString(), ' ', widthCap[1]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.Score.ToString(), ' ', widthCap[2]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.SolutionsAdded.ToString(), ' ', widthCap[3]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.PossibilitiesRemoved.ToString(), ' ', widthCap[4]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.ScorePercentage()
                    .ToString(CultureInfo.InvariantCulture) + "%", ' ', widthCap[5]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.TotalTimeInSecond()
                    .ToString(CultureInfo.InvariantCulture) + "s", ' ', widthCap[6]) + "|"
                + StringUtility.FillEvenlyWith(report.Tracker.AverageTime()
                    .ToString(CultureInfo.InvariantCulture) + "ms", ' ', widthCap[7]) + "\n");
            result.Append(CrossRow(widthCap));
            
            totalStrategyTime += report.Tracker.TotalTime;
        }

        result.Append($"\nTotal strategy time : {Math.Round((double)totalStrategyTime / 1000, 4)}s\n");

        return result.ToString();
    }

    private static string CrossRow(int[] widthCap)
    {
        return StringUtility.Repeat('-', widthCap[0]) + "+" + StringUtility.Repeat('-', widthCap[1]) + "+"
               + StringUtility.Repeat('-', widthCap[2]) + "+" + StringUtility.Repeat('-', widthCap[3]) + "+"
               + StringUtility.Repeat('-', widthCap[4]) + "+" + StringUtility.Repeat('-', widthCap[5]) + "+"
               + StringUtility.Repeat('-', widthCap[6]) + "+" + StringUtility.Repeat('-', widthCap[7]) + "\n";
    }
}

public class StrategyReport
{
    public string StrategyName { get; }
    public IReadOnlyTracker Tracker { get; }

    public StrategyReport(StrategyInformation strategy)
    {
        StrategyName = strategy.StrategyName;
        Tracker = strategy.Tracker;
    }
}