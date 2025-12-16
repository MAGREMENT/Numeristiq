using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Model.Utility;

namespace Model.Core.Trackers;

public class StatisticsTracker<TSolvingState> : Tracker<TSolvingState>
{
    private readonly List<int> _retransmissions = new();
    private readonly List<StrategyStatistics> _statistics = new();
    public int Count { get; private set; }
    private int _success;
    private int _solverFails;
    
    protected override void OnAttach(ITrackerAttachable<TSolvingState> attachable)
    {
        Prepare(attachable.EnumerateStrategies());
        attachable.StrategyStarted += OnStrategyStart;
        attachable.StrategyEnded += OnStrategyEnd;
        attachable.SolveDone += OnSolveDone;
    }

    protected override void OnDetach(ITrackerAttachable<TSolvingState> attachable)
    {
        attachable.StrategyStarted -= OnStrategyStart;
        attachable.StrategyEnded -= OnStrategyEnd;
        attachable.SolveDone -= OnSolveDone;
    }

    private void Prepare(IEnumerable<Strategy> strategies)
    {
        Count = 0;
        _success = 0;
        _solverFails = 0;
        
        _retransmissions.Clear();
        _statistics.Clear();
        foreach (var strategy in strategies)
        {
            if (strategy.Enabled)
            {
                _retransmissions.Add(_statistics.Count);
                _statistics.Add(new StrategyStatistics(strategy.Name));
            }
            else _retransmissions.Add(-1);
        }
    }

    private void OnStrategyStart(Strategy strategy, int index)
    {
        Get(index)?.StartUsing();
    }

    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        Get(index)?.StopUsing(solutionAdded, possibilitiesRemoved);
    }

    private void OnSolveDone(ISolveResult<TSolvingState> result)
    {
        Count++;
        if (result.IsResultCorrect()) _success++;
        else if (result.HasSolverFailed()) _solverFails++;
    }
    
    public override string ToString()
    {
        var result = new StringBuilder("\n\n");
        _statistics.ToGridReport(result, 8, report =>
        [
            report.Name,
            report.Usage.ToString(),
            report.Score.ToString(),
            report.SolutionsAdded.ToString(),
            report.PossibilitiesRemoved.ToString(),
            report.ScorePercentage().ToString(CultureInfo.InvariantCulture) + "s",
            report.TotalTimeInSecond().ToString(CultureInfo.InvariantCulture) + "s",
            report.AverageTime().ToString(CultureInfo.InvariantCulture) + "ms"
        ], ColumnTitle.Follow("Strategy name", "Usage", "Score", 
            "Solutions added", "Possibilities removed",
            "Score percentage", "Total time", "Average time"));

        result.Append($"Completion rate : {_success} / {Count}\n");
        result.Append($"Solver fails : {_solverFails}\n\n");
        
        
        var totalStrategyTime = 0L;
        foreach (var report in _statistics)
        {
            totalStrategyTime += report.TotalTime;
        }

        result.Append($"\nTotal strategy time : {Math.Round((double)totalStrategyTime / 1000, 4)}s\n");

        return result.ToString();
    }
    public IReadOnlyList<StrategyStatistics> GetStatistics() => _statistics;

    private StrategyStatistics? Get(int initialIndex)
    {
        if (initialIndex < 0 || initialIndex >= _retransmissions.Count) return null;

        var index = _retransmissions[initialIndex];
        if (index < 0 || index >= _statistics.Count) return null;

        return _statistics[index];
    }
}

public class StrategyStatistics
{
    public string Name { get; }
    public int Score { get; private set; }
    public int SolutionsAdded { get; private set; }
    public int PossibilitiesRemoved { get; private set; }
    public int Usage { get; private set; }
    public long TotalTime { get; private set; }
    
    private long _lastStartTime;

    public StrategyStatistics(string name)
    {
        Name = name;
    }
    
    [JsonConstructor]
    public StrategyStatistics(string name, int score, int solutionsAdded, int possibilitiesRemoved, int usage, long totalTime)
    {
        Name = name;
        Score = score;
        SolutionsAdded = solutionsAdded;
        PossibilitiesRemoved = possibilitiesRemoved;
        Usage = usage;
        TotalTime = totalTime;
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