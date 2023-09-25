
using Model.Solver;
using Model.Solver.Helpers;

namespace Model.DeprecatedStrategies.SinglePossibility;

public class CellSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyDifficulty Difficulty { get; } = StrategyDifficulty.Basic;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int i = 0; i < 9; i++) 
        {
            for (int j = 0; j < 9; j++)
            {
                if (strategyManager.Sudoku[i, j] != 0) continue;
                
                if (strategyManager.PossibilitiesAt(i, j).Count == 1)
                {
                    int n = strategyManager.PossibilitiesAt(i, j).GetFirst();
                    strategyManager.AddSolution(n,
                        i, j, this);
                }
            }
        }
    }
}