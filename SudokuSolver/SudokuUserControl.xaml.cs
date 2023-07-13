using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver;

public partial class SudokuUserControl : UserControl
{
    private readonly StackPanel _main;

    private Solver _currentSolver = new(new Sudoku());
    private bool _seePossibilities;

    public delegate void OnReady();
    public event OnReady? IsReady;
    
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
                SudokuCellUserControl current = GetTo(i, j);
                current.UnHighLight();
                
                UpdateCell(current, i, j);
            }
        }
    }

    private void UpdateCell(SudokuCellUserControl current, int row, int col)
    {
        if(_currentSolver.Sudoku[row, col] != 0) current.SetDefinitiveNumber(_currentSolver.Sudoku[row, col]);
        else
        {
            if(_seePossibilities) current.SetPossibilities(_currentSolver.Possibilities[row, col]);
            else current.Void();
        }
    }

    public void SolveSudoku()
    {
        _currentSolver.Solve();
        InitSolver(_currentSolver);
        IsReady?.Invoke();
    }
    
    public void RunAllStrategiesOnce()
    {
        _currentSolver.RunAllStrategiesOnce();
        InitSolver(_currentSolver);
        IsReady?.Invoke();
    }

    public async void RunUntilProgress()
    {
        var changes = _currentSolver.RunUntilProgress();
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                SudokuCellUserControl current = GetTo(i, j);
                current.UnHighLight();
            }
        }

        foreach (var coord in changes)
        {
            SudokuCellUserControl current = GetTo(coord[0], coord[1]);
            current.HighLight();
        }

        await Task.Delay(TimeSpan.FromSeconds(0.5));

        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                SudokuCellUserControl current = GetTo(i, j);
                UpdateCell(current, i, j);
            }
        }

        IsReady?.Invoke();
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

    public List<ISolverLog> GetLogs()
    {
        return _currentSolver.Logs;
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