using Model.Utility;

namespace Model.Tectonic.Strategies;

public class HiddenSingleStrategy : AbstractStrategy
{
    public override void Apply(IStrategyUser strategyUser)
    {
        foreach (var zone in strategyUser.Tectonic.Zones)
        {
            for (int n = 1; n <= zone.Count; n++)
            {
                Cell? target = null;
                foreach (var cell in zone)
                {
                    if (strategyUser.PossibilitiesAt(cell).Contains(n))
                    {
                        if (target is null) target = cell;
                        else
                        {
                            target = null;
                            break;
                        }
                    }
                }

                if (target is not null) ; //Add as definitive
            }
        }
    }
}