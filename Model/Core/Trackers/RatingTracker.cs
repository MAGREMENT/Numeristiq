namespace Model.Core.Trackers;

public class RatingTracker<TStrategy, TSolveResult> : Tracker<TStrategy, TSolveResult> where TStrategy : Strategy
{
    private double total;
    private int count;

    public double Rating => total / count;
    
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
        total = 0;
        count = 0;
    }
    
    private void OnStrategyEnd(TStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        count++;
        total += (int)strategy.Difficulty;
    }
}