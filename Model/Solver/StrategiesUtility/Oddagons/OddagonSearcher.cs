using System.Collections.Generic;
using Model.Solver.StrategiesUtility.Graphs;
using Model.Solver.StrategiesUtility.Oddagons.Algorithms;

namespace Model.Solver.StrategiesUtility.Oddagons;

public static class OddagonSearcher
{
    private static readonly IOddagonSearchAlgorithm _algorithm = new OddagonSearchAlgorithmV1();

    public static List<LinkGraphLoop<CellPossibility>> Search(IStrategyManager strategyManager)
    {
        return _algorithm.Search(strategyManager);
    }
}