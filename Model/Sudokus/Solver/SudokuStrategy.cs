using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Settings;

namespace Model.Sudokus.Solver;

public abstract class SudokuStrategy : ICommitMaker, ISettingCollection
{
    private bool _enabled = true;
    private readonly List<ISetting> _settings = new();
    
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public InstanceHandling InstanceHandling { get; set; }
    public IReadOnlyList<ISetting> Settings => _settings;

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

    protected SudokuStrategy(string name, StrategyDifficulty difficulty, InstanceHandling defaultHandling)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        InstanceHandling = defaultHandling;
    }

    protected void AddSetting(ISetting s)
    {
        _settings.Add(s);
    }
    
    public abstract void Apply(IStrategyUser strategyUser);
    public virtual void OnNewSudoku(IReadOnlySudoku s) { }
    public void TrySetSetting(string name, SettingValue value)
    {
        foreach (var arg in Settings)
        {
            if (!arg.Name.Equals(name)) continue;

            arg.Set(value);
        }
    }

    public void Set(int index, SettingValue value, bool checkValidity = true)
    {
        _settings[index].Set(value, checkValidity);
    }

    public override bool Equals(object? obj)
    {
        return obj is SudokuStrategy ss && ss.Name.Equals(Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
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

public enum InstanceHandling
{
    FirstOnly, UnorderedAll, BestOnly, SortedAll
}