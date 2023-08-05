using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class SudokuUserControl : IHighLighter
{
    private readonly StackPanel _main;

    private Solver _currentSolver = new(new Sudoku());
    private int _logBuffer = -1;

    public delegate void OnReady();
    public event OnReady? IsReady;

    public delegate void OnCellClicked(SudokuCellUserControl sender, int row, int col);
    public event OnCellClicked? CellClickedOn;

    public delegate void OnSolverUpdate(string solverAsString);
    public event OnSolverUpdate? SolverUpdated;
    
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

                int rowForEvent = i;
                int colForEvent = j;
                    toAdd.ClickedOn += (sender) =>
                {
                    CellClickedOn?.Invoke(sender, rowForEvent, colForEvent);
                };
            }
        }
    }

    public void NewSolver(Solver solver)
    {
        _currentSolver = solver;
        RefreshSolver();
    }
    
    private void Update()
    {
        RefreshSolver();
        SolverUpdated?.Invoke(_currentSolver.Sudoku.AsString());
    }

    private void RefreshSolver()
    {
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

    public void AddDefinitiveNumber(int number, int row, int col)
    {
        _currentSolver.SetDefinitiveNumberByHand(number, row, col); 
        Update();
    }
    
    public void RemovePossibility(int number, int row, int col)
    {
        _currentSolver.RemovePossibilityByHand(number, row, col);
        Update();
    }

    private void UpdateCell(SudokuCellUserControl current, int row, int col)
    {
        if(_currentSolver.Sudoku[row, col] != 0) current.SetDefinitiveNumber(_currentSolver.Sudoku[row, col]);
        else current.SetPossibilities(_currentSolver.Possibilities[row, col]);
    }

    public void SolveSudoku()
    {
        _currentSolver.Solve();
        if (_currentSolver.Logs.Count > 0) _logBuffer = _currentSolver.Logs[^1].Id;
        Update();
        IsReady?.Invoke();
    }

    public async void RunUntilProgress()
    { 
        _currentSolver.Solve(true);
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                GetTo(i, j).UnHighLight();
            }
        }

        if (_currentSolver.Logs.Count > 0)
        {
            var current = _currentSolver.Logs[^1];
            if (current.Id != _logBuffer)
            {
                current.CauseHighLighter(this);
                _logBuffer = current.Id;
            }
        }

        await Task.Delay(TimeSpan.FromMilliseconds(500));
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                UpdateCell(GetTo(i, j), i, j);
            }
        }

        IsReady?.Invoke();
    }

    public void ShowCurrent()
    {
        RefreshSolver();
    }

    public void ShowLog(ISolverLog log)
    {
        int n = -1;
        int cursor = 0;
        bool possibility = false;
        List<int> buffer = new();
        while (cursor < log.SolverState.Length)
        {
            char current = log.SolverState[cursor];
            if (current is 'd' or 'p')
            {
                if (buffer.Count > 0)
                {
                    var scuc = GetTo(n / 9, n % 9);
                    if (possibility) scuc.SetPossibilities(buffer);
                    else scuc.SetDefinitiveNumber(buffer[0]);
                    scuc.UnHighLight();
                    
                    buffer.Clear();
                }

                possibility = current == 'p';
                n++;
            }
            else buffer.Add(current - '0');

            cursor++;
        }
        
        var scuc2 = GetTo(n / 9, n % 9);
        if (possibility) scuc2.SetPossibilities(buffer);
        else scuc2.SetDefinitiveNumber(buffer[0]);
        scuc2.UnHighLight();

        log.CauseHighLighter(this);
    }

    public void ClearSudoku()
    {
        _currentSolver = new Solver(new Sudoku());
        Update();
    }

    public List<ISolverLog> GetLogs()
    {
        return _currentSolver.Logs;
    }

    private SudokuCellUserControl GetTo(int row, int col)
    {
        return (SudokuCellUserControl) ((StackPanel)_main.Children[row]).Children[col];
    }

    public void HighLightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).HighLightPossibility(possibility, coloration switch
        {
            ChangeColoration.Change => Colors.Aqua,
            ChangeColoration.CauseOne => Colors.Coral,
            ChangeColoration.CauseTwo => Colors.Red,
            _ => throw new ArgumentException("Wtf")
        });
    }

    public void HighLightCell(int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).HighLight(coloration switch
        {
            ChangeColoration.Change => Colors.Aqua,
            ChangeColoration.CauseOne => Colors.Coral,
            ChangeColoration.CauseTwo => Colors.Red,
            _ => throw new ArgumentException("Wtf")
        });
    }
}