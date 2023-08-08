using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class SolverUserControl : IHighlighter
{
    private readonly Solver _solver = new(new Sudoku());
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

        for (int i = 0; i < 9; i++)
        {
            StackPanel row = (StackPanel)Main.Children[i];
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

    public void NewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        RefreshSolver();
    }
    
    private void Update()
    {
        RefreshSolver();
        SolverUpdated?.Invoke(_solver.Sudoku.AsString());
    }

    private void RefreshSolver()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                CellUserControl current = GetTo(i, j);
                current.UnHighlight();
                
                UpdateCell(current, i, j);
            }
        }
    }

    public void AddDefinitiveNumber(int number, int row, int col)
    {
        _solver.SetDefinitiveNumberByHand(number, row, col); 
        Update();
    }
    
    public void RemovePossibility(int number, int row, int col)
    {
        _solver.RemovePossibilityByHand(number, row, col);
        Update();
    }

    private void UpdateCell(CellUserControl current, int row, int col)
    {
        if(_solver.Sudoku[row, col] != 0) current.SetDefinitiveNumber(_solver.Sudoku[row, col]);
        else current.SetPossibilities(_solver.Possibilities[row, col]);
    }

    public void SolveSudoku()
    {
        _solver.Solve();
        if (_solver.Logs.Count > 0) _logBuffer = _solver.Logs[^1].Id;
        Update();
        IsReady?.Invoke();
    }

    public async void RunUntilProgress()
    { 
        _solver.Solve(true);
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                GetTo(i, j).UnHighlight();
            }
        }

        if (_solver.Logs.Count > 0)
        {
            var current = _solver.Logs[^1];
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
                    scuc.UnHighlight();
                    
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
        scuc2.UnHighlight();

        log.SolverHighLighter(this);
    }

    public void ClearSudoku()
    {
        _solver.SetSudoku(new Sudoku());
        Update();
    }
    
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).HighlightPossibility(possibility, ColorUtil.ToColor(coloration));
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        GetTo(row, col).Highlight(ColorUtil.ToColor(coloration));
    }

    public List<ISolverLog> GetLogs()
    {
        return _solver.Logs;
    }

    public IStrategy[] GetStrategies()
    {
        return _solver.Strategies;
    }

    public void ExcludeStrategy(int number)
    {
        _solver.ExcludeStrategy(number);
    }

    public void UseStrategy(int number)
    {
        _solver.UseStrategy(number);
    }

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)Main.Children[row]).Children[col];
    }

}