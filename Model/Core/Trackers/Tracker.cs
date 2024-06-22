using System.Collections.Generic;

namespace Model.Core.Trackers;

public abstract class Tracker<TStrategy, TSolveResult> where TStrategy : Strategy
{
    private ITrackerAttachable<TStrategy, TSolveResult>? _currentAttachable;

    public void AttachTo(ITrackerAttachable<TStrategy, TSolveResult> attachable)
    {
        if (_currentAttachable is not null) return;
        
        _currentAttachable = attachable;
        OnAttach(attachable);
    }

    protected abstract void OnAttach(ITrackerAttachable<TStrategy, TSolveResult> attachable);

    public void Detach()
    {
        if (_currentAttachable is null) return;

        OnDetach(_currentAttachable);
        _currentAttachable = null;
    }
    
    protected abstract void OnDetach(ITrackerAttachable<TStrategy, TSolveResult> attachable);
}

public interface ITrackerAttachable<out TStrategy, out TSolvingState> where TStrategy : Strategy
{
    public event OnSolveStart? SolveStarted;
    public event OnStrategyStart<TStrategy>? StrategyStarted;
    public event OnStrategyEnd<TStrategy>? StrategyEnded;
    public event OnSolveDone<ISolveResult<TSolvingState>>? SolveDone;

    public IEnumerable<TStrategy> EnumerateStrategies();
}

public delegate void OnSolveStart();
public delegate void OnStrategyStart<in TStrategy>(TStrategy strategy, int index) where TStrategy : Strategy;
public delegate void OnStrategyEnd<in TStrategy>(TStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved) where TStrategy : Strategy;
public delegate void OnSolveDone<in TResult>(TResult result);

