using System.Linq;
using Model.Solver;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.StrategiesUtil.LinkGraph.ConstructRules;

public class CellStrongLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();

                linkGraph.AddLink(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }
}