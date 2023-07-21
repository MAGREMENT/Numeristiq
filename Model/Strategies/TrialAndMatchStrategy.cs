using System.Collections.Generic;

namespace Model.Strategies;

public class TrialAndMatchStrategy : IStrategy
{
    public string Name { get; } = "Trial and match";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.ByTrial;
    public int Score { get; set; }

    private readonly int _maxNumberOfPossibility;

    public TrialAndMatchStrategy(int num)
    {
        _maxNumberOfPossibility = num;
    }
    
    public void ApplyOnce(ISolverView solverView)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Sudoku[row, col] == 0 &&
                    solverView.Possibilities[row, col].Count <= _maxNumberOfPossibility)
                {
                    if(ApplyChanges(solverView, RunSimulation(solverView, row, col,
                        solverView.Possibilities[row, col]))) return;
                }
            }
        }
    }

    private int[,] RunSimulation(ISolverView solverView, int row, int col, IEnumerable<int> possibilities)
    {
        int[,]? commonChanges = null;

        foreach (var possibility in possibilities)
        {
            Solver simulation = solverView.Copy();
            simulation.ExcludeStrategy(typeof(TrialAndMatchStrategy));
            simulation.AddDefinitiveNumber(possibility, row, col, this);
            simulation.Solve();

            if (commonChanges is null)
            {
                commonChanges = new int[9, 9];

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (solverView.Sudoku[r, c] == 0 && simulation.Sudoku[r, c] != 0)
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

    private bool ApplyChanges(ISolverView solverView, int[,] toApply)
    {
        bool wasProgressMade = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (toApply[r, c] != 0 && solverView.AddDefinitiveNumber(toApply[r, c], r, c, 
                        this)) wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }
}