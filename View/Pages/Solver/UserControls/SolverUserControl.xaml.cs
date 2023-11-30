using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Global;
using Global.Enums;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class SolverUserControl
{
    public const int CellSize = 60;
    private const int LineWidth = 3;

    private readonly SolverBackgroundManager _backgroundManager;
    
    public LinkOffsetSidePriority SidePriority
    {
        get => _backgroundManager.SidePriority;
        set => _backgroundManager.SidePriority = value;
    }

    public delegate void OnCellSelection(Cell cell);
    public event OnCellSelection? CellSelected;

    public delegate void OnCellUnSelection();
    public event OnCellUnSelection? CellUnselected;

    public delegate void OnChangeCurrentCell(int number);
    public event OnChangeCurrentCell? CurrentCellChangeAsked;

    public delegate void OnRemoveSolutionFromCurrentCell();
    public event OnRemoveSolutionFromCurrentCell? RemoveSolutionFromCurrentCellAsked;

    public SolverUserControl()
    {
        InitializeComponent();

        Main.Focusable = true;

        //Init background
        _backgroundManager = new SolverBackgroundManager(CellSize, LineWidth);
        Main.Width = _backgroundManager.Size;
        Main.Height = _backgroundManager.Size;
        UpdateBackground();
        
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
            var vertical = new TextBlock
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

                int rowForEvent = i;
                int colForEvent = j;
                toAdd.MouseLeftButtonDown += (_, _) =>
                {
                    CellSelected?.Invoke(new Cell(rowForEvent, colForEvent));
                };

                row.Children.Add(toAdd);
            }
        }

        Main.KeyDown += KeyPressed;
        Main.LostFocus += (_, _) => CellUnselected?.Invoke();
    }

    public void SetCellTo(int row, int col, int number)
    {
        GetTo(row, col).SetNumber(number);
    }

    public void SetCellTo(int row, int col, int[] possibilities)
    {
        GetTo(row, col).SetPossibilities(possibilities);
    }

    public void UpdateGivens(HashSet<Cell> givens)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                GetTo(row, col).SetForeground(givens.Contains(new Cell(row, col))
                    ? CellForegroundType.Given
                    : CellForegroundType.Solving);
            }
        }
    }

    public void PutCursorOn(Cell cell)
    {
        _backgroundManager.PutCursorOn(cell.Row, cell.Column);
        Main.Focus();
    }

    public void ClearCursor()
    {
        _backgroundManager.ClearCursor();
        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(Main), null);
        Keyboard.ClearFocus();
    }

    public void UpdateBackground()
    {
        Main.Background = _backgroundManager.Background;
    }

    public void ClearBackground()
    {
        _backgroundManager.Clear();
    }
    
    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration)
    {
        _backgroundManager.FillPossibility(row, col, possibility, ColorManager.ToColor(coloration));
    }

    public void FillCell(int row, int col, ChangeColoration coloration)
    {
        _backgroundManager.FillCell(row, col, ColorManager.ToColor(coloration));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        _backgroundManager.EncirclePossibility(row, col, possibility);
    }

    public void EncircleCell(int row, int col)
    {
        _backgroundManager.EncircleCell(row, col);
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration color)
    {
        _backgroundManager.EncircleRectangle(rowFrom, colFrom, possibilityFrom, rowTo, colTo, 
            possibilityTo, ColorManager.ToColor(color));
    }

    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration)
    {
        _backgroundManager.EncircleCellPatch(cells, ColorManager.ToColor(coloration));
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength)
    {
        _backgroundManager.CreateLink(rowFrom, colFrom, possibilityFrom, rowTo, colTo, possibilityTo, strength == LinkStrength.Weak);
    }

    private void KeyPressed(object? sender, KeyEventArgs args)
    {
        switch (args.Key)
        {
            case Key.NumPad1 : 
                CurrentCellChangeAsked?.Invoke(1);
                break;
            case Key.NumPad2 : 
                CurrentCellChangeAsked?.Invoke(2);
                break;
            case Key.NumPad3 : 
                CurrentCellChangeAsked?.Invoke(3);
                break;
            case Key.NumPad4 : 
                CurrentCellChangeAsked?.Invoke(4);
                break;
            case Key.NumPad5 : 
                CurrentCellChangeAsked?.Invoke(5);
                break;
            case Key.NumPad6 : 
                CurrentCellChangeAsked?.Invoke(6);
                break;
            case Key.NumPad7 : 
                CurrentCellChangeAsked?.Invoke(7);
                break;
            case Key.NumPad8 : 
                CurrentCellChangeAsked?.Invoke(8);
                break;
            case Key.NumPad9 : 
                CurrentCellChangeAsked?.Invoke(9);
                break;
            case Key.NumPad0 :
                RemoveSolutionFromCurrentCellAsked?.Invoke();
                break;
            case Key.Back :
                RemoveSolutionFromCurrentCellAsked?.Invoke();
                break;
        }
    }

    private CellUserControl GetTo(int row, int col)
    {
        return (CellUserControl) ((StackPanel)Main.Children[row]).Children[col];
    }
}

