using System.Collections.Generic;

namespace Model.Strategies;

public class TrialAndMatchStrategy : IStrategy //TODO fix with  4  8  9   5349 7      3s4s1  3  28   4   6   89  1  5s4s2s6s6 8942   2  3  6
{
    private readonly int _maxNumberOfPossibility;

    public TrialAndMatchStrategy(int num)
    {
        _maxNumberOfPossibility = num;
    }
    
    public void ApplyOnce(ISolver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] == 0 &&
                    solver.Possibilities[row, col].Count <= _maxNumberOfPossibility)
                {
                    if(ApplyChanges(solver, RunSimulation(solver, row, col,
                        solver.Possibilities[row, col]))) return;
                }
            }
        }
    }

    private int[,] RunSimulation(ISolver solver, int row, int col, IEnumerable<int> possibilities)
    {
        int[,]? commonChanges = null;

        foreach (var possibility in possibilities)
        {
            ISolver simulation = solver.Copy();
            simulation.ExcludeStrategy(typeof(TrialAndMatchStrategy));
            simulation.AddDefinitiveNumber(possibility, row, col);
            simulation.Solve();

            if (commonChanges is null)
            {
                commonChanges = new int[9, 9];

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (solver.Sudoku[r, c] == 0 && simulation.Sudoku[r, c] != 0)
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

    private static bool ApplyChanges(ISolver solver, int[,] toApply)
    {
        bool wasProgressMade = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (toApply[r, c] != 0 && solver.AddDefinitiveNumber(toApply[r, c], r, c,
                    new TrialAndMatchLog(toApply[r, c], r, c))) wasProgressMade = true;
            }
        }

        return wasProgressMade;
    }
}

public class TrialAndMatchLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.ByTrial;

    public TrialAndMatchLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} added as definitive by trial and match";
    }
}