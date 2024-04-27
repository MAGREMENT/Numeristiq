using Model.Helpers;
using Model.Helpers.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellWeakLinkConstructRule : IConstructRule<ISudokuStrategyUser, ISudokuElement>
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyUser.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = strategyUser.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.Add(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuStrategyUser strategyUser)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(strategyUser.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = strategyUser.PossibilitiesAt(row, col).ToArray();
                for (int i = 0; i < asArray.Length; i++)
                {
                    for (int j = i + 1; j < asArray.Length; j++)
                    {
                        linkGraph.Add(new CellPossibility(row, col, asArray[i]),
                            new CellPossibility(row, col, asArray[j]), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}