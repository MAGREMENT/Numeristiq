using System.Collections.Generic;
using Model.Core;
using Model.Core.Settings;

namespace Model.Sudokus.Solver;

public abstract class SudokuStrategy : Strategy, ISettingCollection
{
    private readonly List<ISetting> _settings = new();
    
    public IReadOnlyList<ISetting> Settings => _settings;

    protected SudokuStrategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling) 
        : base(name, difficulty, defaultHandling) { }

    protected void AddSetting(ISetting s)
    {
        _settings.Add(s);
    }
    
    public abstract void Apply(ISudokuSolverData solverData);
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