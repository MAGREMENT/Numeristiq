using System.Collections.Generic;
using Model.Helpers;
using Model.Sudoku.Solver.Settings;

namespace Model.Sudoku.Solver;

public abstract class AbstractSudokuStrategy : ISudokuStrategy
{ 
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public abstract OnCommitBehavior DefaultOnCommitBehavior { get; }
    public IReadOnlyList<ISetting> Settings => ModifiableSettings.ToArray();

    protected List<ISetting> ModifiableSettings { get; } = new();

    protected AbstractSudokuStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        OnCommitBehavior = defaultBehavior;
    }
    
    public abstract void Apply(IStrategyUser strategyUser);
    public virtual void OnNewSudoku(Sudoku s) { }
    public void TrySetArgument(string name, SettingValue value)
    {
        foreach (var arg in Settings)
        {
            if (!arg.Name.Equals(name)) continue;

            arg.Set(value);
        }
    }
}