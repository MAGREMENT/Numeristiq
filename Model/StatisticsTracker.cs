using System;

namespace Model;

public class StatisticsTracker
{
    public int Score { get; private set; }
    public int Usage { get; private set; }
    public string TimeUsed => (double)_timeUsedMilli / 1000 + " seconds";

    private long _timeUsedMilli;
    private long _lastStartTime;

    public void StartUsing()
    {
        _lastStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    public void StopUsing(bool scored)
    {
        Usage++;
        _timeUsedMilli += DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastStartTime;
        if (scored) Score++;
    }
}