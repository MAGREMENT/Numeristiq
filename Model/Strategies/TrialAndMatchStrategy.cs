﻿using System.Collections.Generic;

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
                    ApplyChanges(solver, RunSimulation(solver, row, col,
                        solver.Possibilities[row, col].All()));
                    return;
                }
            }
        }
    }

    private List<int[]>? RunSimulation(ISolver solver, int row, int col, IEnumerable<int> possibilities)
    {
        List<int[]>? commonChanges = null;

        foreach (var possibility in possibilities)
        {
            Solver simulation = Solver.BasicSolver(solver.Sudoku.Copy());
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

    private static void ApplyChanges(ISolver solver, List<int[]>? toApply)
    {
        if (toApply == null || toApply.Count == 0) return;

        foreach (var change in toApply)
        {
            solver.AddDefinitiveNumber(change[2], change[0], change[1],
                new TrialAndMatchLog(change[2], change[0], change[1]));
        }
    }
}

public class TrialAndMatchLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level => StrategyLevel.Ultimate;

    public TrialAndMatchLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} added as definitive by trial and match";
    }
}