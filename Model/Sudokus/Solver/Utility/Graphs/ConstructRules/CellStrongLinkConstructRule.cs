using Model.Core.Graphs;
using Model.Tectonics.Solver.Utility.ConstructRules;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Graphs.ConstructRules;

public class CellStrongLinkConstructRule : IConstructRule<ISudokuSolverData, ISudokuElement>, 
    IConstructRule<ISudokuSolverData, CellPossibility>
{
    public static CellStrongLinkConstructRule Instance { get; } = new();
    
    private CellStrongLinkConstructRule() {}
    
    public int ID { get; } = UniqueID.Next();

    public void Apply(ILinkGraph<ISudokuElement> linkGraph, ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solverData.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = solverData.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ISudokuSolverData solverData)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if(solverData.PossibilitiesAt(row, col).Count != 2) continue;

                var asArray = solverData.PossibilitiesAt(row, col).ToArray();

                linkGraph.Add(new CellPossibility(row, col, asArray[0]),
                    new CellPossibility(row, col, asArray[1]), LinkStrength.Strong);
            }
        }
    }
}