namespace Model.Strategies.ForcingChains;

public class CellForcingChainStrategy : IStrategy
{
    public string Name => "Cell forcing chain";
    public StrategyLevel Difficulty => StrategyLevel.Extreme;
    public int Score { get; set; }
    public void ApplyOnce(ISolverView solverView)
    {
        //TODO
    }
}