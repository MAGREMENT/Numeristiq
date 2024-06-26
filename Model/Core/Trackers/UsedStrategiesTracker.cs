using System.Collections.Generic;

namespace Model.Core.Trackers;

public class UsedStrategiesTracker : Tracker<object>
{
    private readonly HashSet<string> _used = new();
    
    protected override void OnAttach(ITrackerAttachable<object> attachable)
    {
        attachable.SolveStarted += OnSolveStart;
        attachable.StrategyEnded += OnStrategyEnd;
    }

    protected override void OnDetach(ITrackerAttachable<object> attachable)
    {
        attachable.SolveStarted -= OnSolveStart;
        attachable.StrategyEnded -= OnStrategyEnd;
    }

    private void OnSolveStart()
    {
        _used.Clear();
    }

    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        _used.Add(strategy.Name);
    }

    public bool WasUsed(string strategyName) => _used.Contains(strategyName);
}