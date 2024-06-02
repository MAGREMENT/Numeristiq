using System.Collections.Generic;
using Model.Core;
using Model.Utility.Collections;

namespace Model.Sudokus.Solver;

public class StrategyManager<TStrategy> where TStrategy : Strategy
{
    private readonly UniqueList<TStrategy> _strategies = new();
    
    public bool UniquenessDependantStrategiesAllowed { get; private set; } = true;
    public IReadOnlyList<TStrategy> Strategies => _strategies;

    public void ClearStrategies()
    {
        _strategies.Clear();
    }
    
    public void AddStrategy(TStrategy strategy)
    {
        _strategies.Add(strategy, i => InterchangeStrategies(i, _strategies.Count));
    }

    public void AddStrategy(TStrategy strategy, int position)
    {
        if (position == _strategies.Count)
        {
            AddStrategy(strategy);
            return;
        }

        _strategies.InsertAt(strategy, position, i => InterchangeStrategies(i, position));
    }
    
    public void AddStrategies(IReadOnlyList<TStrategy>? strategies)
    {
        if (strategies is null) return;
        
        foreach (var s in strategies)
        {
            _strategies.Add(s);
        }
    }

    public void AddStrategies(params TStrategy[] strategies)
    {
        foreach (var s in strategies)
        {
            _strategies.Add(s);
        }
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

    public void AllowUniqueness(bool yes)
    {
        UniquenessDependantStrategiesAllowed = yes;
        foreach (var s in Strategies)
        {
            if (s.UniquenessDependency != UniquenessDependency.FullyDependent) continue;

            s.Enabled = yes;
            s.Locked = !yes;
        }
    }
}