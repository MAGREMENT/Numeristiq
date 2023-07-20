using System.Linq;

namespace Model.DeprecatedStrategies.SinglePossibility;

public class CellSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Basic;
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int i = 0; i < 9; i++) 
        {
            for (int j = 0; j < 9; j++)
            {
                if (solverView.Sudoku[i, j] != 0) continue;
                
                if (solverView.Possibilities[i, j].Count == 1)
                {
                    int n = solverView.Possibilities[i, j].First();
                    solverView.AddDefinitiveNumber(n,
                        i, j, this);
                }
            }
        }
    }
}