using System.Timers;
using Model.Utility;

namespace Model.Sudokus.Player;

public class PlayerTimer : ISubscribableTimer
{
    private const int MillisPerCall = 10;
    
    private Timer? _timer;
    private int _millisElapsed;
    
    public event OnTimeElapsed? TimeElapsed;
    public bool IsPlaying => _timer is not null;
    public bool HasTimeElapsed => _millisElapsed != 0;

    public void Start()
    {
        DestroyTimer();
        _millisElapsed = 0;
        CreateTimer();
        
        TimeElapsed?.Invoke(new TimeQuantity(_millisElapsed));
    }

    public void Pause()
    {
        DestroyTimer();
        
        TimeElapsed?.Invoke(new TimeQuantity(_millisElapsed));
    }

    public void Play()
    {
        if(!HasTimeElapsed) Start();
        else CreateTimer();
        
        TimeElapsed?.Invoke(new TimeQuantity(_millisElapsed));
    }

    public void Stop()
    {
        DestroyTimer();
        _millisElapsed = 0;
        
        TimeElapsed?.Invoke(new TimeQuantity(_millisElapsed));
    }

    private void CreateTimer()
    {
        if(_timer is not null) return;
        
        _timer = new Timer(MillisPerCall);
        _timer.AutoReset = true;
        _timer.Elapsed += OnTimedEvent;
        _timer.Enabled = true;
    }

    private void DestroyTimer()
    {
        if (_timer is null) return;
        
        _timer.Enabled = false;
        _timer.Dispose();
        _timer = null;
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        _millisElapsed += MillisPerCall;
        TimeElapsed?.Invoke(new TimeQuantity(_millisElapsed));
    }
}

public interface ISubscribableTimer
{
    public event OnTimeElapsed? TimeElapsed;
    
    public bool IsPlaying { get; }
}

public delegate void OnTimeElapsed(TimeQuantity quantity);

public readonly struct TimeQuantity
{
    public int Hours { get; }
    public int Minutes { get; }
    public int Seconds { get; }
    public int MilliSeconds { get; }

    public TimeQuantity(int milliSeconds)
    {
        MilliSeconds = milliSeconds % 1000;
        var s = milliSeconds / 1000;
        Seconds = s % 60;
        var m = s / 60;
        Minutes = m % 60;
        Hours = m / 60;
    }

    public override string ToString()
    {
        return $"{Hours}:{Minutes.ToString().FillLeftWith('0', 2)}" +
               $":{Seconds.ToString().FillLeftWith('0', 2)}:{MilliSeconds.ToString().FillLeftWith('0', 3)}";
    }
}