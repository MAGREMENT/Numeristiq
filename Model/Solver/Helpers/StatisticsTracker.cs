using System;

namespace Model.Solver.Helpers;

public class StatisticsTracker : IReadOnlyTracker
{
    public int Score { get; private set; }
    public int Usage { get; private set; }
    public long TotalTime { get; private set; }
    
    private long _lastStartTime;

    public void StartUsing()
    {
        _lastStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void StopUsing(bool scored)
    {
        Usage++;
        TotalTime += DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastStartTime;
        if (scored) Score++;
    }
}

public interface IReadOnlyTracker
{
    public int Score { get; }
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