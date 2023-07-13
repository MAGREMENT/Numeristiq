
namespace Model;

public abstract class StrategyPackage : IStrategy
{
    private readonly IStrategy[] _strategies;

    protected StrategyPackage(params IStrategy[] strategies)
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