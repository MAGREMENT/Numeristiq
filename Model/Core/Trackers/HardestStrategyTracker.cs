namespace Model.Core.Trackers;

public class HardestStrategyTracker<TStrategy, TSolveResult> : Tracker<TStrategy, TSolveResult> where TStrategy : Strategy
{
    private int _hardestIndex = -1;
    public TStrategy? Hardest { get; private set; }
    
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
        Hardest = null;
        _hardestIndex = -1;
    }

    private void OnStrategyEnd(TStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        if (Hardest is null || Hardest.Difficulty < strategy.Difficulty
                            || Hardest.Difficulty == strategy.Difficulty && index > _hardestIndex)
        {
            Hardest = strategy;
            _hardestIndex = index;
        }
    }
}