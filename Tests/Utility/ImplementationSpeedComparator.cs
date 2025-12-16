using System.Diagnostics;
using Model.Utility;

namespace Tests.Utility;

public static class ImplementationSpeedComparator
{
    public static void Compare<T>(IReadOnlyList<(T, string)> implementations, int repeatCount, Test<T> test, TearDown<T>? tearDown = null)
    {
        var stopWatch = new Stopwatch();
        var nanosPerTick = 1000L * 1000L * 1000L / Stopwatch.Frequency;
        var result = new SpeedTestResult[implementations.Count];
        
        for(int i = 0; i < implementations.Count; i++)
        {
            var implementation = implementations[i];
            result[i] = new SpeedTestResult(implementation.Item2, repeatCount, nanosPerTick);
            
            for (int n = 0; n <= repeatCount; n++)
            {
                stopWatch.Restart();
                test(implementation.Item1);
                stopWatch.Stop();
                tearDown?.Invoke(implementation.Item1);

                result[i].AddEntry(n, stopWatch.ElapsedTicks);
            }
        }
        
        Console.WriteLine(result.ToGridReport(6, str => [
            str.Name,
            str.TicksToString(str.TotalTicks),
            str.TicksToString(str.GetAverageTicks()),
            str.TicksToString(str.MinTicks),
            str.TicksToString(str.MaxTicks),
            str.TicksToString(str.Dry)
        ], ColumnTitle.Follow("Name", "Total", "Average", "Minimum", "Maximum", "Dry")));
    }
}

public class SpeedTestResult
{
    public long MinTicks { get; private set; } = long.MaxValue;
    public long MaxTicks { get; private set; }
    public long TotalTicks { get; private set; }
    public long Dry { get; private set; } = -1;
    public string Name { get; }
    
    private long _minIndex = -1;
    private long _maxIndex = -1;
    private readonly int _repeatCount;
    private readonly long _nanosPerTick;

    public SpeedTestResult(string name, int repeatCount, long nanosPerTick)
    {
        Name = name;
        _repeatCount = repeatCount;
        _nanosPerTick = nanosPerTick;
    }

    public void AddEntry(int entryNumber, long ticks)
    {
        if (Dry < 0)
        {
            Dry = ticks;
            return;
        }
        
        TotalTicks += ticks;
        if (ticks < MinTicks)
        {
            MinTicks = ticks;
            _minIndex = entryNumber;
        }

        if (ticks > MaxTicks)
        {
            MaxTicks = ticks;
            _maxIndex = entryNumber;
        }
    }

    public double GetAverageTicks()
    {
        return (double)TotalTicks / _repeatCount;
    }
    
    public string TicksToString(double ticks)
    {
        var nanos = ticks * _nanosPerTick;
        return nanos switch
        {
            < 1_000 => nanos + " ns",
            < 1_000_000 => nanos / 1_000 + " mis",
            < 1_000_000_000 => nanos / 1_000_000 + " ms",
            _ => nanos / 1_000_000_000 + " s"
        };
    }

    public string TicksToString(long ticks)
    {
        var nanos = ticks * _nanosPerTick;
        return nanos switch
        {
            < 1_000 => nanos + " ns",
            < 1_000_000 => (double)nanos / 1_000 + " mis",
            < 1_000_000_000 => (double)nanos / 1_000_000 + " ms",
            _ => (double)nanos / 1_000_000_000 + " s"
        };
    }
}

public delegate void Test<in T>(T value);
public delegate void TearDown<in T>(T value);