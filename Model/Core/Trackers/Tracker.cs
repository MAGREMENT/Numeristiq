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

