namespace Model.Core.Trackers;

public class RatingTracker : Tracker<object>, IRatingTracker
{
    private double total;
    private int count;

    public double Rating => total / count;
    
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
        total = 0;
        count = 0;
    }
    
    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        count++;
        total += (int)strategy.Difficulty;
    }
}

public interface IRatingTracker
{
    public double Rating { get; }
    void Detach();
}