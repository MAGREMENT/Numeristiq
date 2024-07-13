using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Model.Utility;

namespace Model.Core.Settings;

public abstract class SettingValue
{
    public virtual bool ToBool()
    {
        return default;
    }

    public virtual int ToInt()
    {
        return default;
    }

    public virtual double ToDouble()
    {
        return default;
    }
    
    public virtual MinMax ToMinMax()
    {
        return default;
    }
    
    protected static bool TranslateBoolean(string s)
    {
        return s.ToLower() == "true";
    }

    protected static int TranslateInt(string s)
    {
        return int.TryParse(s, out var result) ? result : default;
    }

    protected static double TranslateDouble(string s)
    {
        return double.TryParse(s, out var result) ? result : default;
    }

    protected static MinMax TranslateMinMax(string s)
    {
        var split = s.Split(',');
        if (split.Length != 2) return default;

        try
        {
            return new MinMax(int.Parse(split[0]), int.Parse(split[1]));
        }
        catch (Exception)
        {
            return default;
        }
    }
}

public class StringSettingValue : SettingValue
{
    private readonly string _s;

    public StringSettingValue(string s)
    {
        _s = s;
    }

    public override bool ToBool()
    {
        return TranslateBoolean(_s);
    }

    public override int ToInt()
    {
        return TranslateInt(_s);
    }

    public override double ToDouble()
    {
        return TranslateDouble(_s);
    }

    public override MinMax ToMinMax()
    {
        return TranslateMinMax(_s);
    }

    public override string ToString()
    {
        return _s;
    }

    public override bool Equals(object? obj)
    {
        return obj is SettingValue value && _s.Equals(value.ToString());
    }

    public override int GetHashCode()
    {
        return _s.GetHashCode();
    }
}

public class IntSettingValue : SettingValue
{
    private readonly int _i;

    public IntSettingValue(int i)
    {
        _i = i;
    }

    public IntSettingValue(string s)
    {
        _i = TranslateInt(s);
    }

    public override int ToInt()
    {
        return _i;
    }

    public override double ToDouble()
    {
        return _i;
    }

    public override string ToString()
    {
        return _i.ToString();
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SettingValue value && _i == value.ToInt();
    }

    public override int GetHashCode()
    {
        return _i.GetHashCode();
    }
}

public class DoubleSettingValue : SettingValue
{
    private readonly double _d;

    public DoubleSettingValue(double d)
    {
        _d = d;
    }

    public DoubleSettingValue(string s)
    {
        _d = TranslateDouble(s);
    }

    public override double ToDouble()
    {
        return _d;
    }

    public override int ToInt()
    {
        return (int)_d;
    }

    public override string ToString()
    {
        return Math.Round(_d, 2).ToString(CultureInfo.CurrentCulture);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SettingValue value && Math.Abs(_d - value.ToDouble()) < 0.00001;
    }

    public override int GetHashCode()
    {
        return _d.GetHashCode();
    }
}

public class BoolSettingValue : SettingValue
{
    private readonly bool _b;

    public BoolSettingValue(bool b)
    {
        _b = b;
    }

    public BoolSettingValue(string s)
    {
        _b = TranslateBoolean(s);
    }

    public override bool ToBool()
    {
        return _b;
    }

    public override string ToString()
    {
        return _b.ToString();
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SettingValue value && _b == value.ToBool();
    }

    public override int GetHashCode()
    {
        return _b.GetHashCode();
    }
}

public class MinMaxSettingValue : SettingValue
{
    private readonly MinMax _minMax;

    public MinMaxSettingValue(MinMax minMax)
    {
        _minMax = minMax;
    }

    public MinMaxSettingValue(string s)
    {
        TranslateMinMax(s);
    }

    public override MinMax ToMinMax()
    {
        return _minMax;
    }

    public override string ToString()
    {
        return _minMax.ToString();
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SettingValue value && _minMax == value.ToMinMax();
    }

    public override int GetHashCode()
    {
        return _minMax.GetHashCode();
    }
}