using Model.Solver;
using Model.Solver.Helpers;

namespace Model.DeprecatedStrategies.SinglePossibility;

public class RowSinglePossibilityStrategy : IStrategy
{
    public string Name { get; } = "Single possibility";
    
    public StrategyDifficulty Difficulty { get; } = StrategyDifficulty.Basic;
    public StatisticsTracker Tracker { get; } = new();

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int n = 1; n <= 9; n++)
            {
                int pos = CheckRowForUnique(strategyManager, row, n);
                if (pos != -1)
                {
                    strategyManager.AddSolution(n, row, pos, this);
                }
            } 
        }
    }

    private int CheckRowForUnique(IStrategyManager strategyManager, int row, int number)
    {
        int buffer = -1;

        for (int i = 0; i < 9; i++)
        {
            if (strategyManager.Sudoku[row, i] == number) return -1;
            if (strategyManager.PossibilitiesAt(row, i).Peek(number) && strategyManager.Sudoku[row, i] == 0)
            {
                if (buffer != -1) return -1;
                buffer = i;
            }
        }

        return buffer;
    }
}