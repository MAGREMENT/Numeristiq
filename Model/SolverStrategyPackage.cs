namespace Model;

public abstract class SolverStrategyPackage : ISolverStrategy
{
    private readonly ISolverStrategy[] _strategies;

    protected SolverStrategyPackage(params ISolverStrategy[] strategies)
    {
        _strategies = strategies;
    }
    
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        foreach (var strategy in _strategies)
        {
            if (strategy.ApplyOnce(solver)) wasProgressMade = true;
        }

        return wasProgressMade;
    }
}