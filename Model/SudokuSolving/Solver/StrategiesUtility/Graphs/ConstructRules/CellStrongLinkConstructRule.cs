using Global.Enums;

namespace Model.SudokuSolving.Solver.StrategiesUtility.Graphs.ConstructRules;

public class CellStrongLinkConstructRule : IConstructRule
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyManager.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = strategyManager.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }
}