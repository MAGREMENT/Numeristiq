using System;
using System.Collections.Generic;

namespace Model.Sudoku.Solver;

public class StatisticsTracker
{
    private readonly ISolver _solver;

    private readonly List<int> _retransmissions = new();
    private readonly List<SudokuStatistics> _statistics = new();

    public IReadOnlyList<IReadOnlyStatistics> Statistics => _statistics;

    public StatisticsTracker(ISolver solver)
    {
        _solver = solver;

        _solver.StrategyStarted += StartedUsing;
        _solver.StrategyStopped += StoppedUsing;
    }

    public void Prepare()
    {
        _retransmissions.Clear();
        _statistics.Clear();

        var infos = _solver.GetStrategyInfo();

        foreach (var info in infos)
        {
            if (info.Used)
            {
                _retransmissions.Add(_statistics.Count);
                _statistics.Add(new SudokuStatistics(info.StrategyName));
            }
            else _retransmissions.Add(-1);
        }
    }

    private void StartedUsing(int n)
    {
        Get(n)?.StartUsing();
    }

    private void StoppedUsing(int n, int sol, int pos)
    {
        Get(n)?.StopUsing(sol, pos);
    }

    private SudokuStatistics? Get(int initialIndex)
    {
        if (initialIndex < 0 || initialIndex >= _retransmissions.Count) return null;

        var index = _retransmissions[initialIndex];
        if (index < 0 || index >= _statistics.Count) return null;

        return _statistics[index];
    }
}

public class SudokuStatistics : IReadOnlyStatistics
{
    public string Name { get; }
    public int Score { get; private set; }
    public int SolutionsAdded { get; private set; }
    public int PossibilitiesRemoved { get; private set; }
    public int Usage { get; private set; }
    public long TotalTime { get; private set; }
    
    private long _lastStartTime;

    public SudokuStatistics(string name)
    {
        Name = name;
    }

    public void StartUsing()
    {
        _lastStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void StopUsing(int solutionsAdded, int possibilitiesRemoved)
    {
        Usage++;
        TotalTime += DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastStartTime;
        SolutionsAdded += solutionsAdded;
        PossibilitiesRemoved += possibilitiesRemoved;
        if (solutionsAdded + possibilitiesRemoved > 0) Score++;
    }
}

public interface IReadOnlyStatistics
{
    public string Name { get; }
    public int Score { get; }
    public int SolutionsAdded { get; }
    public int PossibilitiesRemoved { get; }
    public int Usage { get; }
    public long TotalTime { get; }

    public double ScorePercentage()
    {
        if (Usage == 0) return 0;
        return Math.Round((double)Score / Usage * 100, 2);  
    }

    public double AverageTime()
    {
        if (Usage == 0) return 0;
        return Math.Round((double)TotalTime / Usage, 4);
    }

    public double TotalTimeInSecond()
    {
        return Math.Round((double)TotalTime / 1000, 4);
    }
}