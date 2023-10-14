using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Model.Solver;
using Model.Solver.Helpers;

namespace Model;

public class RunTester //TODO : Add eliminations + solutions for each strategy + total time spent
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

    public void StartAsync()
    {
        Task.Run(Run);
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

    public override string ToString()
    {
        string[] columnTitles = { "Strategy name", "Usage", "Score", "Score percentage", "Total time", "Average time" };
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
            widthCap[3] = Math.Max(widthCap[3], report.Tracker.ScorePercentage()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[4] = Math.Max(widthCap[4], report.Tracker.TotalTimeInSecond()
                .ToString(CultureInfo.InvariantCulture).Length + 3);
            widthCap[5] = Math.Max(widthCap[5], report.Tracker.AverageTime()
                .ToString(CultureInfo.InvariantCulture).Length + 4);
        }

        var totalWidth = columnTitles.Length - 1;

        foreach (var width in widthCap)
        {
            totalWidth += width;
        }

        var result = new StringBuilder(StringUtil.FillEvenlyWith("Result", '-', totalWidth) + "\n\n");

        result.Append($"Completion rate : {Success} / {Count}\n");
        result.Append($"Solver fails : {SolverFails}\n\n");
        
        result.Append(StringUtil.FillEvenlyWith(columnTitles[0], ' ', widthCap[0]) + "|"
            + StringUtil.FillEvenlyWith(columnTitles[1], ' ', widthCap[1]) + "|"
            + StringUtil.FillEvenlyWith(columnTitles[2], ' ', widthCap[2]) + "|"
            + StringUtil.FillEvenlyWith(columnTitles[3], ' ', widthCap[3]) + "|"
            + StringUtil.FillEvenlyWith(columnTitles[4], ' ', widthCap[4]) + "|"
            + StringUtil.FillEvenlyWith(columnTitles[5], ' ', widthCap[5]) + "\n");
        result.Append(CrossRow(widthCap));

        foreach (var report in Reports)
        {
            result.Append(StringUtil.FillEvenlyWith(report.StrategyName, ' ', widthCap[0]) + "|"
                + StringUtil.FillEvenlyWith(report.Tracker.Usage.ToString(), ' ', widthCap[1]) + "|"
                + StringUtil.FillEvenlyWith(report.Tracker.Score.ToString(), ' ', widthCap[2]) + "|"
                + StringUtil.FillEvenlyWith(report.Tracker.ScorePercentage()
                    .ToString(CultureInfo.InvariantCulture) + "%", ' ', widthCap[3]) + "|"
                + StringUtil.FillEvenlyWith(report.Tracker.TotalTimeInSecond()
                    .ToString(CultureInfo.InvariantCulture) + "s", ' ', widthCap[4]) + "|"
                + StringUtil.FillEvenlyWith(report.Tracker.AverageTime()
                    .ToString(CultureInfo.InvariantCulture) + "ms", ' ', widthCap[5]) + "\n");
            result.Append(CrossRow(widthCap));
        }

        return result.ToString();
    }

    private static string CrossRow(int[] widthCap)
    {
        return StringUtil.Repeat('-', widthCap[0]) + "+" + StringUtil.Repeat('-', widthCap[1]) + "+"
               + StringUtil.Repeat('-', widthCap[2]) + "+" + StringUtil.Repeat('-', widthCap[3]) + "+"
               + StringUtil.Repeat('-', widthCap[4]) + "+" + StringUtil.Repeat('-', widthCap[5]) + "\n";
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