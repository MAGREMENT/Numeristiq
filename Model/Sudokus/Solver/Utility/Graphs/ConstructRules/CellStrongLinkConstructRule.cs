using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellStrongLinkConstructRule : IConstructRule<ISudokuStrategyUser, ISudokuElement>
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyUser.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = strategyUser.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyUser.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = strategyUser.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }
}