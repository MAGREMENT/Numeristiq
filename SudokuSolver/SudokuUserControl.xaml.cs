using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver;

public partial class SudokuUserControl : UserControl
{
    private readonly StackPanel _main;

    private Solver _currentSolver = new(new Sudoku());
    private bool _seePossibilities;
    
    public SudokuUserControl()
    {
        InitializeComponent();

        _main = (FindName("Main") as StackPanel)!;

        for (int i = 0; i < 9; i++)
        {
            StackPanel row = (StackPanel)_main.Children[i];
            for (int j = 0; j < 9; j++)
            {
                var toAdd = new SudokuCellUserControl();
                switch (i)
                {
                    case 2 : case 5 :
                        toAdd.BorderBottom = true;
                        break;
                    case 3 : case 6 :
                        toAdd.BorderTop = true;
                        break;
                }

                switch (j)
                {
                    case 2 : case 5 :
                        toAdd.BorderRight = true;
                        break;
                    case 3 : case 6 :
                        toAdd.BorderLeft = true;
                        break;
                }
                row.Children.Add(toAdd);
            }
        }
    }

    public void InitSolver(Solver solver)
    {
        _currentSolver = solver;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (i == 1 && j == 6)
                {
                    int a = 0;
                }
                SudokuCellUserControl current = GetTo(i, j);
                if(solver.Sudoku[i, j] != 0) current.SetDefinitiveNumber(solver.Sudoku[i, j]);
                else
                {
                    if(_seePossibilities) current.SetPossibilities(solver.Possibilities[i, j].GetPossibilities());
                    else current.Void();
                }
            }
        }
    }

    public void UpdateIfDifferent(string asString)
    {
        if(!_currentSolver.Sudoku.AsString().Equals(asString)) InitSolver(new Solver(new Sudoku(asString)));
    }

    public void SolveSudoku()
    {
        _currentSolver.Solve();
        InitSolver(_currentSolver);
    }

    public void ClearSudoku()
    {
        _currentSolver = new Solver(new Sudoku());
        InitSolver(_currentSolver);
    }

    public string SudokuAsString()
    {
        return _currentSolver.Sudoku.AsString();
    }

    public bool SeePossibilities
    {
        set
        {
            _seePossibilities = value;
            InitSolver(_currentSolver);
        }
    }

    private SudokuCellUserControl GetTo(int row, int col)
    {
        return (SudokuCellUserControl) ((StackPanel)_main.Children[row]).Children[col];
    }
}