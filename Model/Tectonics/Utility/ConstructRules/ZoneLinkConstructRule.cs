using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Tectonics.Utility.ConstructRules;

public class ZoneLinkConstructRule : IConstructRule<ITectonicStrategyUser, ITectonicElement>
{
    public void Apply(ILinkGraph<ITectonicElement> linkGraph, ITectonicStrategyUser strategyUser)
    {
        for (int i = 0; i < strategyUser.Tectonic.Zones.Count; i++)
        {
            var zone = strategyUser.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = strategyUser.ZonePositionsFor(i, n);
                if (pos.Count == 2)
                {
                    int a = -1;
                    pos.Next(ref a);
                    var first = new CellPossibility(zone[a], n);
                    pos.Next(ref a);
                    linkGraph.Add(first, new CellPossibility(zone[a], n), LinkStrength.Strong);
                }
                else if (pos.Count > 2)
                {
                    var asArray = pos.ToArray();
                    for (int j = 0; j < asArray.Length - 1; j++)
                    {
                        for (int k = j + 1; k < asArray.Length; k++)
                        {
                            linkGraph.Add(new CellPossibility(zone[j], n), new CellPossibility(zone[k], n), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ITectonicStrategyUser strategyUser)
    {
        for (int i = 0; i < strategyUser.Tectonic.Zones.Count; i++)
        {
            var zone = strategyUser.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = strategyUser.ZonePositionsFor(i, n);
                if (pos.Count == 2)
                {
                    int a = -1;
                    pos.Next(ref a);
                    var first = new CellPossibility(zone[a], n);
                    pos.Next(ref a);
                    linkGraph.Add(first, new CellPossibility(zone[a], n), LinkStrength.Strong);
                }
                else if (pos.Count > 2)
                {
                    var asArray = pos.ToArray();
                    for (int j = 0; j < asArray.Length - 1; j++)
                    {
                        for (int k = j + 1; k < asArray.Length; k++)
                        {
                            linkGraph.Add(new CellPossibility(zone[j], n),
                                new CellPossibility(zone[k], n), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}