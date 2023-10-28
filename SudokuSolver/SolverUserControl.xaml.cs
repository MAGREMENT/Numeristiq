using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.Helpers.Logs;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Solver.StrategiesUtil.LinkGraph;
using SudokuSolver.Utils;

namespace SudokuSolver;

public partial class SolverUserControl : IHighlightable, ISolverGraphics
{
    public const int CellSize = 60;
    private const int LineWidth = 3;

    private readonly Solver _solver = new(new Sudoku())
    {
        LogsManaged = true,
        StatisticsTracked = false
    };
    private int _logBuffer;

    public SudokuTranslationType TranslationType { get; set; } = SudokuTranslationType.Shortcuts;

    public SolverState StartState => _solver.StartState;
    public SolverState CurrentState => _solver.State;
    public int DelayBefore { get; set; } = 350;
    public int DelayAfter { get; set; } = 350;

    private readonly SolverBackgroundManager _backgroundManager;

    public delegate void OnReady();
    public event OnReady? IsReady;

    public delegate void OnCellClicked(CellUserControl sender, int row, int col);
    public event OnCellClicked? CellClickedOn;

    public delegate void OnSudokuAsStringChanged(string solverAsString);
    public event OnSudokuAsStringChanged? SudokuAsStringChanged;

    public event LogManager.OnLogsUpdated? LogsUpdated; 

    public event OnLogShowed? LogShowed;
    public event OnCurrentStateShowed? CurrentStateShowed;

    public SolverUserControl()
    {
        InitializeComponent();

        _solver.LogsUpdated += logs => LogsUpdated?.Invoke(logs);

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
            var horizontal = new TextBlock()
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
                toAdd.ClickedOn += sender =>
                {
                    CellClickedOn?.Invoke(sender, rowForEvent, colForEvent);
                    _backgroundManager.PutCursorOn(rowForEvent, colForEvent);
                    Main.Background = _backgroundManager.Background;
                };
            }
        }

        ShowCurrent();
    }

    public void NewSudoku(Sudoku sudoku)
    {
        _solver.SetSudoku(sudoku);
        ShowCurrent();

        _logBuffer = 0;
    }
    
    private void Update()
    {
        ShowCurrent();
        SudokuAsStringChanged?.Invoke(SudokuTranslator.Translate(_solver.Sudoku, TranslationType));
    }
    
    private void UpdateCell(CellUserControl current, int row, int col)
    {
        if(_solver.Sudoku[row, col] != 0) current.SetDefinitiveNumber(_solver.Sudoku[row, col]);
        else current.SetPossibilities(_solver.PossibilitiesAt(row, col));
    }

    public void AddDefinitiveNumber(int number, int row, int col)
    {
        _solver.SetSolutionByHand(number, row, col); 
        Update();
    }
    
    public void RemovePossibility(int number, int row, int col)
    {
        _solver.RemovePossibilityByHand(number, row, col);
        Update();
    }
    
    public void ClearSudoku()
    {
        _solver.SetSudoku(new Sudoku());
        Update();
    }

    public async void SolveSudoku()
    {
        await _solver.SolveAsync();
        
        if (_solver.Logs.Count > 0) _logBuffer = _solver.Logs[^1].Id;
        Update();
        IsReady?.Invoke();
    }

    public async void RunUntilProgress()
    {
        await _solver.SolveAsync(true);
        
        for (int n = _logBuffer; n < _solver.Logs.Count; n++)
        {
            _backgroundManager.Clear();
            
            var current = _solver.Logs[n];
            
            Highlight(current);
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
        
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                UpdateCell(GetTo(i, j), i, j);
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
    
    public void HighLightLog(ISolverLog log)
    {
        _backgroundManager.Clear();
        log.HighlightManager.Highlight(this);
        Main.Background = _backgroundManager.Background;
    }

    private void Highlight(ISolverLog log)
    {
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

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)Main.Children[row]).Children[col];
    }
    
    //IHighlightable----------------------------------------------------------------------------------------------------
    public void HighlightPossibility(int possibility, int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightPossibility(row, col, possibility, ColorUtil.ToColor(coloration));
    }

    public void CirclePossibility(int possibility, int row, int col)
    {
        _backgroundManager.CirclePossibility(row, col, possibility);
    }

    public void HighlightCell(int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.HighlightCell(row, col, ColorUtil.ToColor(coloration));
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
                _backgroundManager.HighlightPossibility(coord.Row, coord.Col, coord.Possibility, ColorUtil.ToColor(coloration));
                break;
            case PointingRow pr :
                _backgroundManager.HighlightGroup(pr, ColorUtil.ToColor(coloration));
                break;
            case PointingColumn pc :
                _backgroundManager.HighlightGroup(pc, ColorUtil.ToColor(coloration));
                break;
            case AlmostNakedSet anp :
                _backgroundManager.HighlightGroup(anp, ColorUtil.ToColor(coloration));
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

