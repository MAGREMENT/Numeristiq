using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.Oddagons.Algorithms;

public class OddagonSearchAlgorithmV1 : IOddagonSearchAlgorithm
{
    public List<LinkGraphLoop<CellPossibility>> Search(IStrategyManager strategyManager)
    {
        strategyManager.GraphManager.ConstructSimple(ConstructRule.CellStrongLink, ConstructRule.CellWeakLink,
            ConstructRule.UnitStrongLink, ConstructRule.UnitWeakLink);
        var graph = strategyManager.GraphManager.SimpleLinkGraph;
        var result = new List<LinkGraphLoop<CellPossibility>>();

        return result;
    }
}