using System;
using System.Collections.Generic;
using Global;
using Model.Solver.Helpers;

namespace Model.Solver;

public interface IStrategy
{ 
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public UniquenessDependency UniquenessDependency { get; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public OnCommitBehavior DefaultOnCommitBehavior { get; }
    public StatisticsTracker Tracker { get; }
    public IReadOnlyList<IStrategyArgument> Arguments { get; }
    
    void Apply(IStrategyManager strategyManager);
    void OnNewSudoku(Sudoku s);
    void TrySetArgument(string name, string value);
    public Dictionary<string, string> ArgumentsAsDictionary()
    {
        Dictionary<string, string> result = new();

        foreach (var arg in Arguments)
        {
            result.Add(arg.Name, arg.Get());
        }

        return result;
    }
}

public enum StrategyDifficulty
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}

public enum UniquenessDependency
{
    NotDependent, PartiallyDependent, FullyDependent
}

public enum OnCommitBehavior
{
    Return, WaitForAll, ChooseBest
}

public class IntStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }

    private readonly GetArgument<int> _getter;
    private readonly SetArgument<int> _setter;
    
    public IntStrategyArgument(string name, GetArgument<int> getter, SetArgument<int> setter, IArgumentViewInterface i)
    {
        Name = name;
        Interface = i;
        _getter = getter;
        _setter = setter;
    }

    public string Get()
    {
        return _getter().ToString();
    }

    public void Set(string value)
    {
        try
        {
            var asInt = int.Parse(value);
            _setter(asInt);
        }
        catch
        {
            // ignored
        }
    }
}

public class MinMaxIntStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    
    private readonly GetArgument<int> _minGetter;
    private readonly SetArgument<int> _minSetter;
    private readonly GetArgument<int> _maxGetter;
    private readonly SetArgument<int> _maxSetter;

    public MinMaxIntStrategyArgument(string name, int minMin, int minMax, int maxMin, int maxMax, int tickFrequency, 
        GetArgument<int> minGetter, SetArgument<int> minSetter, GetArgument<int> maxGetter, SetArgument<int> maxSetter)
    {
        Name = name;
        _minSetter = minSetter;
        _maxGetter = maxGetter;
        _maxSetter = maxSetter;
        _minGetter = minGetter;
        Interface = new MinMaxSliderViewInterface(minMin, minMax, maxMin, maxMax, tickFrequency);
    }
    
    public string Get()
    {
        return $"{_minGetter()},{_maxGetter()}";
    }

    public void Set(string s)
    {
        var i = s.IndexOf(',');
        if (i == -1) return;

        try
        {
            var min = int.Parse(s[..i]);
            var max = int.Parse(s[(i + 1)..]);
            _minSetter(min);
            _maxSetter(max);
        }
        catch
        {
            // ignored
        }
    }
}

public class BooleanStrategyArgument : IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    
    private readonly GetArgument<bool> _getter;
    private readonly SetArgument<bool> _setter;

    public BooleanStrategyArgument(string name, GetArgument<bool> getter, SetArgument<bool> setter)
    {
        Name = name;
        Interface = new BooleanViewInterface();
        _getter = getter;
        _setter = setter;
    }
    
    public string Get()
    {
        return _getter().ToString();
    }
    
    public void Set(string s)
    {
        bool val;
        switch (s.ToLower())
        {
            case "true" : val = true;
                break;
            case "false" : val = false;
                break;
            default: return;
        }

        _setter(val);
    }
}

public interface IStrategyArgument
{
    public string Name { get; }
    public IArgumentViewInterface Interface { get; }
    public string Get();
    public void Set(string s);
}