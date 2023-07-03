using System.Collections.Generic;

namespace Model.Strategies;

public class TrialAndMatchStrategy : IStrategy
{
    private readonly int _maxNumberOfPossibility;

    public TrialAndMatchStrategy(int num)
    {
        _maxNumberOfPossibility = num;
    }
    
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<int> poss;
                if (solver.Sudoku[row, col] == 0 &&
                    (poss = solver.Possibilities[row, col].GetPossibilities()).Count <= _maxNumberOfPossibility)
                {
                    if (ApplyChanges(solver, RunSimulation(solver, row, col, poss))) wasProgressMade = true;
                }
            }
        }

        return wasProgressMade;
    }

    public bool ApplyUntilProgress(ISolver solver)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                List<int> poss;
                if (solver.Sudoku[row, col] == 0 &&
                    (poss = solver.Possibilities[row, col].GetPossibilities()).Count <= _maxNumberOfPossibility)
                {
                    if (ApplyChanges(solver, RunSimulation(solver, row, col, poss))) return true;
                }
            }
        }

        return false;
    }

    private List<int[]>? RunSimulation(ISolver solver, int row, int col, List<int> possibilities)
    {
        List<int[]>? commonChanges = null;

        foreach (var possibility in possibilities)
        {
            Solver simulation = new Solver(solver.Sudoku.Copy());
            simulation.Strategies.RemoveAt(3); //TODO make responsive
            simulation.AddDefinitiveNumber(possibility, row, col);
            simulation.Solve();

            if (commonChanges is null)
            {
                commonChanges = new List<int[]>();

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (solver.Sudoku[r, c] == 0 && simulation.Sudoku[r, c] != 0)
                        {
                            commonChanges.Add(new[] { r, c, simulation.Sudoku[r, c] });
                        }
                    }
                }
            }
            else
            {
                for(int i = 0; i < commonChanges.Count; i++)
                {
                    int[] current = commonChanges[i];
                    if (simulation.Sudoku[current[0], current[1]] != current[2])
                    {
                        commonChanges.RemoveAt(i);
                    }
                }
            }
        }

        return commonChanges;
    }

    private static bool ApplyChanges(ISolver solver, List<int[]>? toApply)
    {
        if (toApply == null || toApply.Count == 0) return false;

        foreach (var change in toApply)
        {
            solver.AddDefinitiveNumber(change[2], change[0], change[1],
                new TrialAndMatchLog(change[2], change[0], change[1]));
        }

        return true;
    }
}

public class TrialAndMatchLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Hard;

    public TrialAndMatchLog(int number, int row, int col)
    {
        AsString = $"{number} added in row {row}, column {col} by trial and match";
    }
}