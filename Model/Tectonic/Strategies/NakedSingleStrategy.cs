namespace Model.Tectonic.Strategies;

public class NakedSingleStrategy : AbstractStrategy
{
    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var cell in strategyUser.Tectonic.EachCell())
        {
            var candidates = strategyUser.PossibilitiesAt(cell);
            if (candidates.Count == 1)
            {
                //Make definitive
            }
        }
    }
}