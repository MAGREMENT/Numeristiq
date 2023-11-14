using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Changes;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Helpers.Logs;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;
using View.Utils;

namespace View;

public partial class SolverUserControl : IHighlightable, ISolverGraphics
{
    public const int CellSize = 60;
    private const int LineWidth = 3;

    private readonly SudokuSolver _solver = new()
    {
        LogsManaged = true,
        StatisticsTracked = false
    };
    private int _logBuffer;
    private Cell? _currentlyFocusedCell;

    public SudokuTranslationType TranslationType { get; set; } = SudokuTranslationType.Shortcuts;

    public SolverState StartState => _solver.StartState;
    public SolverState CurrentState => _solver.State;
    public int DelayBefore { get; set; } = 350;
    public int DelayAfter { get; set; } = 350;
    public bool UniquenessAllowed => _solver.UniquenessDependantStrategiesAllowed;
    public OnInstanceFound OnInstanceFound => _solver.OnInstanceFound;

    private readonly SolverBackgroundManager _backgroundManager;

    public delegate void OnReady();
    public event OnReady? IsReady;

    public delegate void OnSudokuChanged(string sudokuAsString);
    public event OnSudokuChanged? SudokuChanged;

    public event LogManager.OnLogsUpdated? LogsUpdated;

    public event Model.Solver.SudokuSolver.OnCurrentStrategyChange? CurrentStrategyChanged;

    public event OnLogShowed? LogShowed;
    public event OnCurrentStateShowed? CurrentStateShowed;

    public SolverUserControl()
    {
        InitializeComponent();

        _solver.LogsUpdated += logs => LogsUpdated?.Invoke(logs);
        _solver.CurrentStrategyChanged += index => CurrentStrategyChanged?.Invoke(index);

        //Init background
        _backgroundManager = new SolverBackgroundManager(CellSize, LineWidth);
        Main.Width = _backgroundManager.Size;
        Main.Height = _backgroundManager.Size;
        
        //Init numbers
        for (int i = 0; i < 9; i++)
        {
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(LineWidth)
            });
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(CellSize)
            });
            var horizontal = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = (i + 1).ToString(),
                FontSize = 15,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(horizontal, 1 + i * 2);
            HorizontalNumbers.Children.Add(horizontal);
            
            VerticalNumbers.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(LineWidth)
            });
            VerticalNumbers.RowDefinitions.Add(new RowDefinition()
            {
                Height = new GridLength(CellSize)
            });
            var vertical = new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = (i + 1).ToString(),
                FontSize = 15,
                FontWeight = FontWeights.Bold
            };
            Grid.SetRow(vertical, 1 + i * 2);
            VerticalNumbers.Children.Add(vertical);
        }
        
        //Init cells
        for (int i = 0; i < 9; i++)
        {
            StackPanel row = (StackPanel)Main.Children[i];
            for (int j = 0; j < 9; j++)
            {
                var toAdd = new CellUserControl();
                toAdd.SetMargin(LineWidth, LineWidth, 0, 0);
                row.Children.Add(toAdd);

                int rowForEvent = i;
                int colForEvent = j;
                toAdd.ClickedOn += _ =>
                {
                    if (toAdd.IsFocused)
                    {
                        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(toAdd), null);
                        Keyboard.ClearFocus();
                    }
                    else toAdd.Focus();
                };

                toAdd.Focusable = true;
                toAdd.GotFocus += (_, _) =>
                {
                    _backgroundManager.PutCursorOn(rowForEvent, colForEvent);
                    Main.Background = _backgroundManager.Background;
                    var cell = new Cell(rowForEvent, colForEvent);
                    _currentlyFocusedCell = cell;
                };
                
                toAdd.LostFocus += (_, _) =>
                {
                    _backgroundManager.PutCursorOn(rowForEvent, colForEvent);
                    Main.Background = _backgroundManager.Background;
                    _currentlyFocusedCell = null;
                };
            }
        }

        KeyDown += KeyPressed;
        
        ShowCurrent();
        UpdateForegroundColors();
    }

    public void NewSudoku(Sudoku sudoku)
    {
        SetSudoku(sudoku);
        ShowCurrent();

        _logBuffer = 0;
    }

    private void SetSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        UpdateForegroundColors();
    }
    
    private void UpdateCellDisplay()
    {
        ShowCurrent();
        SudokuChanged?.Invoke(SudokuTranslator.Translate(_solver.Sudoku, TranslationType));
    }

    public void ClearSudoku()
    {
        SetSudoku(new Sudoku());
        UpdateCellDisplay();
    }

    public async void SolveSudoku()
    {
        await _solver.SolveAsync();
        
        if (_solver.Logs.Count > 0) _logBuffer = _solver.Logs[^1].Id;
        UpdateCellDisplay();
        IsReady?.Invoke();
    }

    public async void RunUntilProgress()
    {
        await _solver.SolveAsync(true);
        
        for (int n = _logBuffer; n < _solver.Logs.Count; n++)
        {
            var current = _solver.Logs[n];
            
            HighlightLog(current);
            ShowStateWithoutUpdatingBackground(current.StateBefore);
            LogShowed?.Invoke(current);
            await Task.Delay(TimeSpan.FromMilliseconds(DelayBefore));
            
            ShowStateWithoutUpdatingBackground(current.StateAfter);
            await Task.Delay(TimeSpan.FromMilliseconds(DelayAfter));

            _logBuffer = current.Id;
        }
        
        ShowCurrent();
        CurrentStateShowed?.Invoke();
        
        IsReady?.Invoke();
    }
    
    public void ShowCurrent()
    {
        _backgroundManager.Clear();
        Main.Background = _backgroundManager.Background;
        
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var current = GetTo(row, col);
                
                if(_solver.Sudoku[row, col] != 0) current.SetDefinitiveNumber(_solver.Sudoku[row, col]);
                else current.SetPossibilities(_solver.PossibilitiesAt(row, col));
            }
        }
    }

    public void ShowState(SolverState state)
    {
        _backgroundManager.Clear();
        Main.Background = _backgroundManager.Background;

        ShowStateWithoutUpdatingBackground(state);
    }

    private void ShowStateWithoutUpdatingBackground(SolverState state)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                var uc = GetTo(row, col);
                var cs = state.At(row, col);

                if (cs.IsPossibilities) uc.SetPossibilities(cs.AsPossibilities);
                else uc.SetDefinitiveNumber(cs.AsNumber);
            }
        }
    }
    
    public void HighlightLog(ISolverLog log)
    {
        _backgroundManager.Clear();
        log.HighlightManager.Highlight(this);
        Main.Background = _backgroundManager.Background;
    }

    public StrategyInfo[] GetStrategies()
    {
        return _solver.StrategyInfos;
    }

    public void ExcludeStrategy(int number)
    {
        _solver.ExcludeStrategy(number);
    }

    public void UseStrategy(int number)
    {
        _solver.UseStrategy(number);
    }

    public void AllowUniqueness(bool yes)
    {
        _solver.AllowUniqueness(yes);
    }

    public void SetOnInstanceFound(OnInstanceFound found)
    {
        _solver.SetOnInstanceFound(found);
    }

    private void KeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.NumPad1 : LiveModification(1);
                break;
            case Key.NumPad2 : LiveModification(2);
                break;
            case Key.NumPad3 : LiveModification(3);
                break;
            case Key.NumPad4 : LiveModification(4);
                break;
            case Key.NumPad5 : LiveModification(5);
                break;
            case Key.NumPad6 : LiveModification(6);
                break;
            case Key.NumPad7 : LiveModification(7);
                break;
            case Key.NumPad8 : LiveModification(8);
                break;
            case Key.NumPad9 : LiveModification(9);
                break;
            case Key.Enter : RemoveSolution();
                break;
            case Key.NumPad0 : RemoveSolution();
                break;
        }
    }

    private void LiveModification(int i)
    {
        if (_currentlyFocusedCell is null) return;

        if (ActionOnKeyboardInput == ChangeType.Solution)
        {
            AddSolution(i, _currentlyFocusedCell.Value.Row, _currentlyFocusedCell.Value.Col);
            UpdateForegroundColors();
        }
        else RemovePossibility(i, _currentlyFocusedCell.Value.Row, _currentlyFocusedCell.Value.Col);
    }

    private void RemoveSolution()
    {
        if (_currentlyFocusedCell is null) return;
        
        _solver.RemoveSolutionByHand(_currentlyFocusedCell.Value.Row, _currentlyFocusedCell.Value.Col);
        UpdateCellDisplay();
        UpdateForegroundColors();
    }
    
    private void AddSolution(int number, int row, int col)
    {
        _solver.SetSolutionByHand(number, row, col); 
        UpdateCellDisplay();
    }
    
    private void RemovePossibility(int number, int row, int col)
    {
        _solver.RemovePossibilityByHand(number, row, col);
        UpdateCellDisplay();
    }

    public ChangeType ActionOnKeyboardInput { get; set; } = ChangeType.Solution;

    public void UpdateForegroundColors()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GetTo(row, col).SetForeground(_solver.StartState[row, col] == 0
                    ? CellForegroundType.Solving
                    : CellForegroundType.Given);
            }
        }
    }

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)Main.Children[row]).Children[col];
    }
    
    //IHighlightable----------------------------------------------------------------------------------------------------
    
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightPossibility(row, col, possibility, ColorManager.ToColor(coloration));
    }

    public void CirclePossibility(int possibility, int row, int col)
    {
        _backgroundManager.CirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightCell(row, col, ColorManager.ToColor(coloration));
    }

    public void CircleCell(int row, int col)
    {
        _backgroundManager.CircleCell(row, col);
    }

    public void HighlightLinkGraphElement(ILinkGraphElement element, ChangeColoration coloration)
    {
        switch (element)
        {
            case CellPossibility coord :
                _backgroundManager.HighlightPossibility(coord.Row, coord.Col, coord.Possibility, ColorManager.ToColor(coloration));
                break;
            case PointingRow pr :
                _backgroundManager.HighlightGroup(pr, ColorManager.ToColor(coloration));
                break;
            case PointingColumn pc :
                _backgroundManager.HighlightGroup(pc, ColorManager.ToColor(coloration));
                break;
            case AlmostNakedSet anp :
                _backgroundManager.HighlightGroup(anp, ColorManager.ToColor(coloration));
                break;
        }
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        _backgroundManager.CreateLink(from, to, linkStrength == LinkStrength.Weak);
    }

    public void CreateLink(ILinkGraphElement from, ILinkGraphElement to, LinkStrength linkStrength)
    {
        switch (from)
        {
            case CellPossibility one when to is CellPossibility two:
                _backgroundManager.CreateLink(one, two, linkStrength == LinkStrength.Weak);
                break;
            case CellPossibility when to is AlmostNakedSet:
                break;
            default:
                CellPossibility[] winners = new CellPossibility[2];
                double winningDistance = int.MaxValue;

                foreach (var c1 in from.EveryCellPossibilities())
                {
                    foreach (var c2 in to.EveryCellPossibilities())
                    {
                        foreach (var pc1 in c1.ToCellPossibility())
                        {
                            foreach (var pc2 in c2.ToCellPossibility())
                            {
                                var distance = Math.Pow(pc1.Row - pc2.Row, 2) + Math.Pow(pc1.Col - pc2.Col, 2);

                                if (distance < winningDistance)
                                {
                                    winningDistance = distance;
                                    winners[0] = pc1;
                                    winners[1] = pc2;
                                }
                            }
                        }
                    }
                }
            
                _backgroundManager.CreateLink(winners[0], winners[1], linkStrength == LinkStrength.Weak);
                break;
        }
    }
}

