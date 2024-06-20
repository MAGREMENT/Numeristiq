using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Utility.ConstructRules;

public class ZoneLinkConstructRule : IConstructRule<ITectonicSolverData, ITectonicElement>
{
    public void Apply(ILinkGraph<ITectonicElement> linkGraph, ITectonicSolverData solverData)
    {
        for (int i = 0; i < solverData.Tectonic.Zones.Count; i++)
        {
            var zone = solverData.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = solverData.ZonePositionsFor(i, n);
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

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ITectonicSolverData solverData)
    {
        for (int i = 0; i < solverData.Tectonic.Zones.Count; i++)
        {
            var zone = solverData.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = solverData.ZonePositionsFor(i, n);
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
                            linkGraph.Add(new CellPossibility(zone[j], n),
                                new CellPossibility(zone[k], n), LinkStrength.Weak);
                        }
                    }
                }
            }
        }
    }
}