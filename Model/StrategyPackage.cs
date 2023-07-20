
namespace Model;

public abstract class StrategyPackage : IStrategy
{
    public string Name { get; } = "Package";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.None;
    
    private readonly IStrategy[] _strategies;

    protected StrategyPackage(params IStrategy[] strategies)
    {
        _strategies = strategies;
    }

    public void ApplyOnce(ISolverView solverView)
    {
        foreach (var strategy in _strategies)
        {
            strategy.ApplyOnce(solverView);
        }
    }
}