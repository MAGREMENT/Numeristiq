namespace Model.Sudoku.Solver.Settings;

public interface ISettingViewInterface
{
    
}

public class SliderViewInterface : ISettingViewInterface
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

public class MinMaxSliderViewInterface : ISettingViewInterface
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



public class BooleanViewInterface : ISettingViewInterface
{
    
}