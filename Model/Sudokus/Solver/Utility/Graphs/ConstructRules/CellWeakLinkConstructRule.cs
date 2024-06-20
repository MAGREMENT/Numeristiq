using Model.Core.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellWeakLinkConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>
{
    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solverData.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = solverData.PossibilitiesAt(row, col).ToArray();
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

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solverData.PossibilitiesAt(row, col).Count < 3) continue;

                var asArray = solverData.PossibilitiesAt(row, col).ToArray();
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