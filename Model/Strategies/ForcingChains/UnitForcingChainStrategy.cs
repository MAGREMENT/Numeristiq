namespace Model.Strategies.ForcingChains;

public class UnitForcingChainStrategy : IStrategy
{
    public string Name => "Unit forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(ISolverView solverView)
    {
        //TODO
    }
}