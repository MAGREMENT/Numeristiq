using System.Diagnostics;

namespace Tests.Utility;

public static class ImplementationSpeedComparator
{
    public static void Compare<T>(Test<T> test, int repeatCount, params T[] implementations)
    {
        Compare(test, repeatCount, (IReadOnlyList<T>)implementations);
    }
    
    public static void Compare<T>(Test<T> test, int repeatCount, IReadOnlyList<T> implementations)
    {
        var stopWatch = new Stopwatch();
        var nanosPerTick = 1000L * 1000L * 1000L / Stopwatch.Frequency;
        
        for(int i = 0; i < implementations.Count; i++)
        {
            var implementation = implementations[i];
            var result = new SpeedTestResult();
            
            for (int n = 0; n <= repeatCount; n++)
            {
                stopWatch.Restart();
                test(implementation);
                stopWatch.Stop();

                result.AddEntry(n, stopWatch.ElapsedTicks);
            }
            
            Console.WriteLine($"#{i + 1} {implementation!.GetType().Name}");
            result.ToConsole(nanosPerTick, repeatCount);
            Console.WriteLine();
        }
    }
}

public class SpeedTestResult
{
    private long _minTicks = long.MaxValue;
    private long _maxTicks;
    private long _minIndex = -1;
    private long _maxIndex = -1;
    private long _totalTicks;
    private long _dry = -1;

    public void AddEntry(int entryNumber, long ticks)
    {
        if (_dry < 0)
        {
            _dry = ticks;
            return;
        }
        
        _totalTicks += ticks;
        if (ticks < _minTicks)
        {
            _minTicks = ticks;
            _minIndex = entryNumber;
        }

        if (ticks > _maxTicks)
        {
            _maxTicks = ticks;
            _maxIndex = entryNumber;
        }
    }

    public void ToConsole(long nanosPerTick, int repeatCount)
    {
        Console.WriteLine($"   Total: {TimeInNanosToString(_totalTicks * nanosPerTick)}");
        Console.WriteLine($"   Average: {TimeInNanosToString((double)(_totalTicks * nanosPerTick) / repeatCount)}");
        Console.WriteLine($"   Minimum: {TimeInNanosToString(_minTicks * nanosPerTick)} on try #{_minIndex + 1}");
        Console.WriteLine($"   Maximum: {TimeInNanosToString(_maxTicks * nanosPerTick)} on try #{_maxIndex + 1}");
        Console.WriteLine($"   Dry: {TimeInNanosToString(_dry * nanosPerTick)}");
    }
    
    private static string TimeInNanosToString(double nanos)
    {
        return nanos switch
        {
            < 1_000 => nanos + " ns",
            < 1_000_000 => nanos / 1_000 + " mis",
            < 1_000_000_000 => nanos / 1_000_000 + " ms",
            _ => nanos / 1_000_000_000 + " s"
        };
    }

    private static string TimeInNanosToString(long nanos)
    {
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