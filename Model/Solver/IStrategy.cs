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
        catch (Exception)
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

public delegate T GetArgument<out T>();
public delegate void SetArgument<in T>(T value);