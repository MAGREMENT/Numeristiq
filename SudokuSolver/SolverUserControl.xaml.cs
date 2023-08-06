using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class SolverUserControl : IHighLighter
{
    private readonly StackPanel _main;

    private Solver _currentSolver = new(new Sudoku());
    private int _logBuffer = -1;

    public delegate void OnReady();
    public event OnReady? IsReady;

    public delegate void OnCellClicked(CellUserControl sender, int row, int col);
    public event OnCellClicked? CellClickedOn;

    public delegate void OnSolverUpdate(string solverAsString);
    public event OnSolverUpdate? SolverUpdated;
    
    public SolverUserControl()
    {
        InitializeComponent();

        _main = (FindName("Main") as StackPanel)!;

        for (int i = 0; i < 9; i++)
        {
            StackPanel row = (StackPanel)_main.Children[i];
            for (int j = 0; j < 9; j++)
            {
                var toAdd = new CellUserControl();
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
                CellUserControl current = GetTo(i, j);
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

    private void UpdateCell(CellUserControl current, int row, int col)
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
                current.SolverHighLighter(this);
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

        log.SolverHighLighter(this);
    }

    public void ClearSudoku()
    {
        _currentSolver = new Solver(new Sudoku());
        Update();
    }
    
    public void HighLightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).HighLightPossibility(possibility, ColorUtil.ToColor(coloration));
    }

    public void HighLightCell(int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).HighLight(ColorUtil.ToColor(coloration));
    }

    public List<ISolverLog> GetLogs()
    {
        return _currentSolver.Logs;
    }

    public IStrategy[] GetStrategies()
    {
        return _currentSolver.Strategies;
    }

    public void ExcludeStrategy(int number)
    {
        _currentSolver.ExcludeStrategy(number);
    }

    public void UseStrategy(int number)
    {
        _currentSolver.UseStrategy(number);
    }

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)_main.Children[row]).Children[col];
    }

}