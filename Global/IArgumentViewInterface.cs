namespace Global;

public interface IArgumentViewInterface
{
    
}

public class SliderViewInterface : IArgumentViewInterface
{
    public SliderViewInterface(int min, int max, int tickFrequency)
    {
        Min = min;
        Max = max;
        TickFrequency = tickFrequency;
    }

    public int Min { get; }
    public int Max { get; }
    public int TickFrequency { get; }
}

public class MinMaxSliderViewInterface : IArgumentViewInterface
{
    public MinMaxSliderViewInterface(int minMin, int minMax, int maxMin, int maxMax, int tickFrequency)
    {
        MinMin = minMin;
        MinMax = minMax;
        MaxMin = maxMin;
        MaxMax = maxMax;
        TickFrequency = tickFrequency;
    }

    public int MinMin { get; }
    public int MinMax { get; }
    public int MaxMin { get; }
    public int MaxMax { get; }
    public int TickFrequency { get; }
}

public readonly struct MinMax
{
    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public static MinMax From(string s)
    {
        var i = s.IndexOf(',');
        if (i == -1) return new MinMax(0, 0);

        try
        {
            return new MinMax(int.Parse(s[..i]), int.Parse(s[(i + 1)..]));
        }
        catch
        {
            return new MinMax(0, 0);
        }
    }

    public override string ToString()
    {
        return $"{Min},{Max}";
    }

    public int Min { get; }
    public int Max { get; }
}

public class BooleanViewInterface : IArgumentViewInterface
{
    
}