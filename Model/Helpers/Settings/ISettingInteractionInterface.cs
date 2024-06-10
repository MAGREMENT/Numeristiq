using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Model.Core;
using Model.Utility;
using Model.Utility.Collections;

namespace Model.Helpers.Settings;

public interface ISettingInteractionInterface
{
    public SettingValue Verify(SettingValue value);
}

public interface IStringListInteractionInterface : ISettingInteractionInterface, IEnumerable<string>
{
    public int[]? IndexTranslator { get; }
}

public class SliderInteractionInterface : ISettingInteractionInterface
{
    public SliderInteractionInterface(double min, double max, double tickFrequency)
    {
        Min = min;
        Max = max;
        TickFrequency = tickFrequency;
    }

    public double Min { get; }
    public double Max { get; }
    public double TickFrequency { get; }
    
    public SettingValue Verify(SettingValue value)
    {
        var asDouble = value.ToDouble();
        return asDouble >= Min && asDouble <= Max && (asDouble - Min) % TickFrequency == 0 ? value : new DoubleSettingValue(Min);
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
    
    public SettingValue Verify(SettingValue value)
    {
        var mm = value.ToMinMax();
        return mm.Min >= MinMin && mm.Min <= MinMax && (mm.Min - MinMin) % TickFrequency == 0
               && mm.Max >= MaxMin && mm.Max <= MaxMax && (mm.Max - MaxMin) % TickFrequency == 0
            ? value
            : new MinMaxSettingValue(new MinMax(MinMin, MaxMax));
    }
}

public class NameListInteractionInterface : IStringListInteractionInterface
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

    public int[]? IndexTranslator => null;
    
    public SettingValue Verify(SettingValue value)
    {
        var asInt = value.ToInt();
        return asInt >= 0 && asInt < _names.Count ? value : new IntSettingValue(0);
    }
}

public class EnumListInteractionInterface<TEnum> : IStringListInteractionInterface where TEnum : struct, Enum
{
    private readonly IEnumerable<string> _enumerable;
    
    public int[] IndexTranslator { get; }

    public EnumListInteractionInterface(IStringConverter? converter)
    {
        var buffer = Enum.GetNames<TEnum>();
        _enumerable = converter is null ? buffer : buffer.Select(converter.Convert);
        IndexTranslator = EnumConverter.ToIntArray<TEnum>();
    }
    
    public IEnumerator<string> GetEnumerator()
    {
        return _enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public SettingValue Verify(SettingValue value)
    {
        return IndexTranslator.Contains(value.ToInt()) ? value : new IntSettingValue(IndexTranslator[0]);
    }
}

public class CheckBoxInteractionInterface : ISettingInteractionInterface
{
    public SettingValue Verify(SettingValue value)
    {
        return value;
    }
}

public class AutoFillingInteractionInterface : ISettingInteractionInterface
{
    private readonly IReadOnlyList<string> _values;

    public AutoFillingInteractionInterface(IReadOnlyList<string> values)
    {
        _values = values;
    }

    public string Fill(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return _values[0];
        
        foreach (var str in _values)
        {
            if (str.Contains(s)) return str;
        }

        return _values[0];
    }

    public SettingValue Verify(SettingValue value)
    {
        var str = value.ToString();
        return _values.Any(s => s.Equals(str)) ? value : new StringSettingValue(_values[0]);
    }
}