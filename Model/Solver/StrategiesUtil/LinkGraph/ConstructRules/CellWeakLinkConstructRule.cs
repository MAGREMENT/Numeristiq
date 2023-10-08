using System.Linq;

namespace Model.Solver.StrategiesUtil.LinkGraph.ConstructRules;

public class CellWeakLinkConstructRule : IConstructRule
{
    public void Apply(LinkGraph<ILinkGraphElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.AddLink(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }

    public void Apply(LinkGraph<CellPossibility> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.AddLink(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}