using System;
using System.Collections.Generic;
using Model.Strategies;
using Model.Strategies.SamePossibilities;
using Model.Strategies.SinglePossibility;

namespace Model;

public class Solver : ISolver
{
    public CellPossibilities[,] Possibilities { get; init; }
    public Sudoku Sudoku { get; }

    private readonly ISolverStrategy[] _strategies =
    {
        new SinglePossibilityStrategyPackage(), new MiniGridDeductionStrategy(), new SamePossibilitiesStrategyPackage()
    };
    
    public delegate void OnSudokuChange();
    public event OnSudokuChange? NumberAdded;

    public Solver(Sudoku s)
    {
        Possibilities = new CellPossibilities[9, 9];
        Sudoku = s;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Possibilities[i, j] = new CellPossibilities();
            }
        }
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (Sudoku[i, j] != 0)
                {
                    UpdatePossibilitiesAfterDefinitiveNumberAdded(s[i, j], i, j);
                }
            }
        }
    }
    
    public bool AddDefinitiveNumber(int number, int row, int col)
    {
        if (Sudoku[row, col] != 0) return false;
        Sudoku[row, col] = number;
        UpdatePossibilitiesAfterDefinitiveNumberAdded(number, row, col);
        NumberAdded?.Invoke();
        return true;
    }

    private void UpdatePossibilitiesAfterDefinitiveNumberAdded(int number, int row, int col)
    {
        for (int i = 0; i < 9; i++)
        {
            Possibilities[row, i].Remove(number);
            Possibilities[i, col].Remove(number);
        }
        
        int startRow = (row / 3) * 3;
        int startColumn = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Possibilities[startRow + i, startColumn + j].Remove(number);
            }
        }
        
        
    }

    public void Solve()
    {
        bool wasProgressMade = true;
        while (wasProgressMade)
        {
            wasProgressMade = false;

            foreach (var strategy in _strategies)
            {
                if (strategy.ApplyOnce(this)) wasProgressMade = true;
                if (Sudoku.IsComplete()) return;
            }
        }
    }

}

