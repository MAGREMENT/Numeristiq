using System;
using Model.Utility;

namespace Model.Sudoku.Solver.Arguments;

public abstract class ArgumentValue
{
    public virtual bool ToBool()
    {
        return default;
    }

    public virtual int ToInt()
    {
        return default;
    }

    public virtual MinMax ToMinMax()
    {
        return default;
    }
}

public class StringArgumentValue : ArgumentValue
{
    private readonly string _s;

    public StringArgumentValue(string s)
    {
        _s = s;
    }

    public override bool ToBool()
    {
        return _s.ToLower() == "true";
    }

    public override int ToInt()
    {
        try
        {
            return int.Parse(_s);
        }
        catch (Exception)
        {
            return default;
        }
    }

    public override MinMax ToMinMax()
    {
        var split = _s.Split();
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

    public override string ToString()
    {
        return _s;
    }
}

public class IntArgumentValue : ArgumentValue
{
    private readonly int _i;

    public IntArgumentValue(int i)
    {
        _i = i;
    }

    public IntArgumentValue(string s)
    {
        try
        {
            _i = int.Parse(s);
        }
        catch (Exception)
        {
            _i = 0;
        }
    }

    public override int ToInt()
    {
        return _i;
    }

    public override string ToString()
    {
        return _i.ToString();
    }
}

public class BoolArgumentValue : ArgumentValue
{
    private readonly bool _b;

    public BoolArgumentValue(bool b)
    {
        _b = b;
    }

    public BoolArgumentValue(string s)
    {
        _b = s.ToLower() == "true";
    }

    public override bool ToBool()
    {
        return _b;
    }

    public override string ToString()
    {
        return _b.ToString();
    }
}

public class MinMaxArgumentValue : ArgumentValue
{
    private readonly MinMax _minMax;

    public MinMaxArgumentValue(MinMax minMax)
    {
        _minMax = minMax;
    }

    public MinMaxArgumentValue(string s)
    {
        var split = s.Split();
        if (split.Length != 2)
        {
            _minMax = default;
            return;
        }

        try
        {
            _minMax = new MinMax(int.Parse(split[0]), int.Parse(split[1]));
        }
        catch (Exception)
        {
            _minMax = default;
        }
    }

    public override MinMax ToMinMax()
    {
        return _minMax;
    }

    public override string ToString()
    {
        return _minMax.ToString();
    }
}