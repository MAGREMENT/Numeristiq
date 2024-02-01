using Model.Utility;

namespace Model.Tectonic.Strategies;

public class HiddenSingleStrategy : AbstractStrategy
{
    public override void Apply(ISolvable solvable)
    {
        foreach (var zone in solvable.Tectonic.Zones)
        {
            for (int n = 1; n <= zone.Count; n++)
            {
                Cell? target = null;
                foreach (var cell in zone)
                {
                    if (solvable.GetCandidates(cell).Peek(n))
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