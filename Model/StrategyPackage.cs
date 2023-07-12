
namespace Model;

public abstract class StrategyPackage : IStrategy
{
    private readonly ISubStrategy[] _strategies;

    protected StrategyPackage(params ISubStrategy[] strategies)
    {
        _strategies = strategies;
    }
    
    public void ApplyOnce(ISolver solver)
    {
        foreach (var strategy in _strategies)
        {
            strategy.ApplyOnce(solver);
        }
    }
}