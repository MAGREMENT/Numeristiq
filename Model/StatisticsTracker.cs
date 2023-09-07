using System;

namespace Model;

public class StatisticsTracker : IReadOnlyTracker
{
    public int Score { get; private set; }
    public int Usage { get; private set; }
    public long TimeUsed { get; private set; }
    
    private long _lastStartTime;

    public void StartUsing()
    {
        _lastStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void StopUsing(bool scored)
    {
        Usage++;
        TimeUsed += DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastStartTime;
        if (scored) Score++;
    }
}

public interface IReadOnlyTracker
{
    public int Score { get; }
    public int Usage { get; }
    public long TimeUsed { get; }

    public double ScorePercentage()
    {
        if (Usage == 0) return 0;
        return (double)Score / Usage * 100;  
    }

    public double AverageTimeUsage()
    {
        if (Usage == 0) return 0;
        return (double)TimeUsed / Usage;
    } 
}