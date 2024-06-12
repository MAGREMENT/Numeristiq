using System.Diagnostics;

namespace Tests.Utility;

public static class ImplementationSpeedComparator
{
    public static void Compare<T>(Test<T> test, int repeatCount, params T[] implementations)
    {
        var stopWatch = new Stopwatch();
        var nanosPerTick = 1000L * 1000L * 1000L / Stopwatch.Frequency;
        
        for(int i = 0; i < implementations.Length; i++)
        {
            var implementation = implementations[i];

            var minTicks = long.MaxValue;
            var maxTicks = 0L;
            var minIndex = -1;
            var maxIndex = -1;
            var totalTicks = 0L;
            var ignored = 0;
            
            for (int n = 0; n <= repeatCount; n++)
            {
                stopWatch.Restart();
                test(implementation);
                stopWatch.Stop();

                if (n == 0 || (maxTicks > 0 && stopWatch.ElapsedTicks > maxTicks * 3))
                {
                    ignored++;
                    continue;
                }
                
                totalTicks += stopWatch.ElapsedTicks;
                if (stopWatch.ElapsedTicks < minTicks)
                {
                    minTicks = stopWatch.ElapsedTicks;
                    minIndex = n;
                }

                if (stopWatch.ElapsedTicks > maxTicks)
                {
                    maxTicks = stopWatch.ElapsedTicks;
                    maxIndex = n;
                }
            }
            
            Console.WriteLine($"#{i + 1} {implementation!.GetType().Name}");
            Console.WriteLine($"   Total: {TimeInNanosToString(totalTicks * nanosPerTick)}");
            Console.WriteLine($"   Average: {TimeInNanosToString((double)(totalTicks * nanosPerTick) / repeatCount)}");
            Console.WriteLine($"   Minimum: {TimeInNanosToString(minTicks * nanosPerTick)} on try #{minIndex + 1}");
            Console.WriteLine($"   Maximum: {TimeInNanosToString(maxTicks * nanosPerTick)} on try #{maxIndex + 1}");
            Console.WriteLine($"   Ignored: {ignored}");
            Console.WriteLine();
        }
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