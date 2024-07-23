using System.Collections.Generic;
using Model.Core.Settings;

namespace Model.Core;

public abstract class Strategy<T> : Strategy
{
    protected Strategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling) 
        : base(name, difficulty, defaultHandling)
    {
    }

    public abstract void Apply(T data);
}

public abstract class Strategy : ISettingCollection
{
    private bool _enabled = true;
    
    public string Name { get; protected init; }
    public StepDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public InstanceHandling InstanceHandling { get; set; }
    
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (!Locked) _enabled = value;
        }
    }

    public bool Locked { get; set; }

    public bool StopOnFirstPush => InstanceHandling == InstanceHandling.FirstOnly;
    
    protected Strategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        InstanceHandling = defaultHandling;
    }

    public virtual IEnumerable<ISetting> EnumerateSettings()
    {
        yield break;
    }
    
    public void TrySetSetting(string name, SettingValue value)
    {
        foreach (var arg in EnumerateSettings())
        {
            if (!arg.Name.Equals(name)) continue;

            arg.Set(value);
        }
    }

    public void Set(int index, SettingValue value, bool checkValidity = true)
    {
        var ind = 0;
        foreach (var setting in EnumerateSettings())
        {
            if (ind++ == index) setting.Set(value, checkValidity);
        }
    }
}

public enum StepDifficulty
{
    None, Basic, Easy, Medium, Hard, Extreme, Inhuman, ByTrial
}

public enum UniquenessDependency
{
    NotDependent, PartiallyDependent, FullyDependent
}

public enum InstanceHandling
{
    FirstOnly, UnorderedAll, BestOnly, SortedAll
}