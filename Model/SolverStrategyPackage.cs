using System.Linq;

namespace Model;

public abstract class SolverStrategyPackage
{
    private readonly ISolverStrategy[] _strategies;

    protected SolverStrategyPackage(params ISolverStrategy[] strategies)
    {
        _strategies = strategies;
    }
    
    public bool ApplyAllOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        foreach (var strategy in _strategies)
        {
            if (strategy.ApplyOnce(solver)) wasProgressMade = true;
        }

        return wasProgressMade;
    }

    public bool ApplyUntilProgress(ISolver solver)
    {
        return _strategies.Any(strategy => strategy.ApplyOnce(solver));
    }
}