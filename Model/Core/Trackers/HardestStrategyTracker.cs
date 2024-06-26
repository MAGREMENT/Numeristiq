namespace Model.Core.Trackers;

public class HardestStrategyTracker : Tracker<object>, IHardestStrategyTracker
{
    private int _hardestIndex = -1;
    public Strategy? Hardest { get; private set; }
    
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
        Hardest = null;
        _hardestIndex = -1;
    }

    private void OnStrategyEnd(Strategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
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

public interface IHardestStrategyTracker
{
    public Strategy? Hardest { get; }
    void Detach();
}