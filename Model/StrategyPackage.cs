
namespace Model;

public abstract class StrategyPackage : IStrategy
{
    private readonly ISubStrategy[] _strategies;

    protected StrategyPackage(params ISubStrategy[] strategies)
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

    public bool ApplyUntilProgress(ISolver solver)
    {
        foreach (var strategy in _strategies)
        {
            if (strategy.ApplyOnce(solver)) return true;
        }
        
        return false;
    }
}