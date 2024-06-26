using System.Collections.Generic;

namespace Model.Core.Trackers;

public abstract class Tracker<TSolveResult>
{
    private ITrackerAttachable<TSolveResult>? _currentAttachable;

    public void AttachTo(ITrackerAttachable<TSolveResult> attachable)
    {
        if (_currentAttachable is not null) return;
        
        _currentAttachable = attachable;
        OnAttach(attachable);
    }

    protected abstract void OnAttach(ITrackerAttachable<TSolveResult> attachable);

    public void Detach()
    {
        if (_currentAttachable is null) return;

        OnDetach(_currentAttachable);
        _currentAttachable = null;
    }
    
    protected abstract void OnDetach(ITrackerAttachable<TSolveResult> attachable);
}

public interface ITrackerAttachable<out TSolvingState>
{
    public event OnSolveStart? SolveStarted;
    public event OnStrategyStart? StrategyStarted;
    public event OnStrategyEnd? StrategyEnded;
    public event OnSolveDone<ISolveResult<TSolvingState>>? SolveDone;

    public IEnumerable<Strategy> EnumerateStrategies();
}

public delegate void OnSolveStart();

public delegate void OnStrategyStart(Strategy strategy, int index);
public delegate void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved);
public delegate void OnSolveDone<in TResult>(TResult result);

