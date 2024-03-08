using System.Collections;
using System.Collections.Generic;
using Model.Utility;

namespace Model.Helpers.Settings;

public interface ISettingInteractionInterface
{
    
}

public class SliderInteractionInterface : ISettingInteractionInterface
{
    public SliderInteractionInterface(int min, int max, int tickFrequency)
    {
        Min = min;
        Max = max;
        TickFrequency = tickFrequency;
    }

    public int Min { get; }
    public int Max { get; }
    public int TickFrequency { get; }
}

public class NameListInteractionInterface : ISettingInteractionInterface, IEnumerable<string>
{
    private readonly IReadOnlyList<INamed> _names;
    
    public NameListInteractionInterface(IReadOnlyList<INamed> names)
    {
        _names = names;
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var n in _names)
        {
            yield return n.Name;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class MinMaxSliderInteractionInterface : ISettingInteractionInterface
{
    public MinMaxSliderInteractionInterface(int minMin, int minMax, int maxMin, int maxMax, int tickFrequency)
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



public class BooleanInteractionInterface : ISettingInteractionInterface
{
    
}