using System.Collections.Generic;
using Model.Helpers.Changes;
using Model.Helpers.Settings;

namespace Model.Sudoku.Solver;

public abstract class SudokuStrategy : ICommitMaker
{
    private bool _enabled = true;
    private bool _locked;
    private List<ISetting> _settings = new();
    
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public abstract OnCommitBehavior DefaultOnCommitBehavior { get; }
    public IReadOnlyList<ISetting> Settings => _settings;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (!_locked) _enabled = value;
        }
    }

    public bool Locked
    {
        get => _locked;
        set
        {
            _locked = value;
            if (_locked) _enabled = false;
        }
    }

    protected SudokuStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        OnCommitBehavior = defaultBehavior;
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