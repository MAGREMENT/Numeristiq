using System.Collections.Generic;
using Model.Sudoku.Solver.Settings;
using Model.Utility;

namespace Model.Sudoku.Solver;

public class StrategyManager
{
    public bool UniquenessDependantStrategiesAllowed { get; private set; } = true;
    private readonly UniqueList<SudokuStrategy> _strategies = new();
    public IReadOnlyList<SudokuStrategy> Strategies => _strategies;

    public void EnableAllStrategies(bool enabled)
    {
        foreach (var strategy in _strategies)
        {
            strategy.Enabled = enabled;
        }
    }
    
    public void AllowUniqueness(bool allowed)
    {
        UniquenessDependantStrategiesAllowed = allowed;
        foreach (var strategy in _strategies)
        {
            if (strategy.UniquenessDependency == UniquenessDependency.FullyDependent) strategy.Locked = !allowed;
        }
    }
    
    public void AddStrategy(SudokuStrategy strategy)
    {
        _strategies.Add(strategy, i => InterchangeStrategies(i, _strategies.Count));
    }

    public void AddStrategies(IReadOnlyList<SudokuStrategy>? strategies)
    {
        if (strategies is null) return;
        
        foreach (var s in strategies)
        {
            _strategies.Add(s);
        }
    }

    public void AddStrategy(SudokuStrategy strategy, int position)
    {
        if (position == _strategies.Count)
        {
            AddStrategy(strategy);
            return;
        }

        _strategies.InsertAt(strategy, position, i => InterchangeStrategies(i, position));
    }
    
    public void RemoveStrategy(int position)
    {
        _strategies.RemoveAt(position);
    }
    
    public void InterchangeStrategies(int positionOne, int positionTwo)
    {
        var buffer = _strategies[positionOne];
        _strategies.RemoveAt(positionOne);
        var newPosTwo = positionTwo > positionOne ? positionTwo - 1 : positionTwo;
        AddStrategy(buffer, newPosTwo);
    }

    public void ChangeStrategyBehavior(string name, OnCommitBehavior behavior)
    {
        var i = _strategies.Find(s => s.Name.Equals(name));
        if (i == -1) return;
        
        _strategies[i].OnCommitBehavior = behavior;
    }

    public void ChangeStrategyBehaviorForAll(OnCommitBehavior behavior)
    {
        foreach (var strategy in _strategies)
        {
            strategy.OnCommitBehavior = behavior;
        }

    }

    public void ChangeStrategyUsage(string name, bool yes)
    {
        var i = _strategies.Find(s => s.Name.Equals(name));
        if (i == -1) return;

        if (yes) _strategies[i].Enabled = yes;
    }
    
    public void ChangeArgument(string strategyName, string argumentName, SettingValue value)
    {
        var i = _strategies.Find(s => s.Name.Equals(strategyName));
        if (i == -1) return;

        _strategies[i].TrySetSetting(argumentName, value);
    }
}