using System.Linq;
using Model.Solver;

namespace Model.StrategiesUtil.LinkGraph.ConstructRules;

public class CellStrongLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.Possibilities[row, col].Count != 2) continue;

                var asArray = strategyManager.Possibilities[row, col].ToArray();

                linkGraph.AddLink(new PossibilityCoordinate(row, col, asArray[0]),
                    new PossibilityCoordinate(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }
}