namespace Model.Solver.Strategies;

public class NoStrategy : AbstractStrategy
{
    public NoStrategy() : base("No Strategy", StrategyDifficulty.None){}

    public override void ApplyOnce(IStrategyManager strategyManager)
    {
        
    }
}