using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Utility.ConstructRules;

public class ZoneLinkConstructionRule : IConstructionRule<ITectonicSolverData, IGraph<ITectonicElement, LinkStrength>>
{
    public static ZoneLinkConstructionRule Instance { get; } = new();
    
    private ZoneLinkConstructionRule(){}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ITectonicElement, LinkStrength> linkGraph, ITectonicSolverData data)
    {
        for (int i = 0; i < data.Tectonic.Zones.Count; i++)
        {
            var zone = data.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = data.ZonePositionsFor(i, n);
                if (pos.Count == 2)
                {
                    var a = pos.NextPosition(-1);
                    linkGraph.Add(new CellPossibility(zone[a], n),
                        new CellPossibility(zone[pos.NextPosition(a)], n), LinkStrength.Strong);
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
}