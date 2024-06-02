using System.Collections.Generic;

namespace Model.Core.Trackers;

public class UsedStrategiesTracker<TStrategy, TSolveResult> : Tracker<TStrategy, TSolveResult> where TStrategy : Strategy
{
    private readonly HashSet<string> _used = new();
    
    protected override void OnAttach(ITrackerAttachable<TStrategy, TSolveResult> attachable)
    {
        attachable.SolveStarted += OnSolveStart;
        attachable.StrategyEnded += OnStrategyEnd;
    }

    protected override void OnDetach(ITrackerAttachable<TStrategy, TSolveResult> attachable)
    {
        attachable.SolveStarted -= OnSolveStart;
        attachable.StrategyEnded -= OnStrategyEnd;
    }

    private void OnSolveStart()
    {
        _used.Clear();
    }

    private void OnStrategyEnd(TStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        _used.Add(strategy.Name);
    }

    public bool WasUsed(string strategyName) => _used.Contains(strategyName);
}