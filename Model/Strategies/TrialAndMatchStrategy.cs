using System.Collections.Generic;
using Model.Solver;

namespace Model.Strategies;

public class TrialAndMatchStrategy : IStrategy //TODO fixme
{
    public string Name { get; } = "Trial and match";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.ByTrial;
    public StatisticsTracker Tracker { get; } = new();

    private readonly int _maxNumberOfPossibility;

    public TrialAndMatchStrategy(int num)
    {
        _maxNumberOfPossibility = num;
    }
    
    public void ApplyOnce(IStrategyManager strategyManager)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 &&
                    strategyManager.Possibilities[row, col].Count <= _maxNumberOfPossibility)
                {
                    if(ApplyChanges(strategyManager, RunSimulation(strategyManager, row, col,
                        strategyManager.Possibilities[row, col]))) return;
                }
            }
        }
    }

    private int[,] RunSimulation(IStrategyManager strategyManager, int row, int col, IEnumerable<int> possibilities)
    {
        int[,]? commonChanges = null;

        foreach (var possibility in possibilities)
        {
            Solver.Solver simulation = strategyManager.Copy();
            simulation.ExcludeStrategies(StrategyLevel.ByTrial);
            simulation.AddSolution(possibility, row, col, this);
            simulation.Solve();

            if (commonChanges is null)
            {
                commonChanges = new int[9, 9];

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (strategyManager.Sudoku[r, c] == 0 && simulation.Sudoku[r, c] != 0)
                        {
                            commonChanges[r, c] = simulation.Sudoku[r, c];
                        }
                    }
                }
            }
            else
            {
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (commonChanges[r, c] != 0 && simulation.Sudoku[r, c] != commonChanges[r, c])
                        {
                            commonChanges[r, c] = 0;
                        }
                    }
                }
            }
        }

        return commonChanges!;
    }

    private bool ApplyChanges(IStrategyManager strategyManager, int[,] toApply)
    {
        bool wasProgressMade = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (toApply[r, c] != 0 && strategyManager.AddSolution(toApply[r, c], r, c, 
                        this)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }
}