using System.Linq;
using Model.Solver;

namespace Model.StrategiesUtil.LinkGraph.ConstructRules;

public class CellWeakLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.Possibilities[row, col].Count < 3) continue;

                var asArray = strategyManager.Possibilities[row, col].ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.AddLink(new PossibilityCoordinate(row, col, asArray[i]),
                            new PossibilityCoordinate(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}