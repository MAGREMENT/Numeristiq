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

public class BooleanViewInterface : IArgumentViewInterface
{
    
}