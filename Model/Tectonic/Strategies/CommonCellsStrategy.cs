namespace Model.Tectonic.Strategies;

public class CommonCellsStrategy : AbstractStrategy
{
    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var zone in strategyUser.Tectonic.Zones)
        {
            
        }
    }
}