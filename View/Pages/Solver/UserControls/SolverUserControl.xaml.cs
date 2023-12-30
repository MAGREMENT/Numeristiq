using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Global;
using Global.Enums;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class SolverUserControl
{
    private const int BigLineWidth = 3;
    private const int SmallLineWidth = 1;
    private const int PossibilitySize = 20;
    
    private readonly SudokuGrid _grid;
    
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

        _grid = new SudokuGrid(PossibilitySize, SmallLineWidth, BigLineWidth);
        ToAddGrid.Children.Add(_grid);
        
        //Init numbers
        for (int i = 0; i < 9; i++)
        {
            var delta = i % 3 == 0 ? BigLineWidth : SmallLineWidth;
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(delta)
            });
            HorizontalNumbers.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(PossibilitySize * 3)
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
            
            VerticalNumbers.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(delta)
            });
            VerticalNumbers.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(PossibilitySize * 3)
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
        
        _grid.KeyDown += KeyPressed;
        _grid.CellSelected += (row, col) => CellSelected?.Invoke(new Cell(row, col));
        _grid.LostFocus += (_, _) => CellUnselected?.Invoke();
    }
    
    public void UpdateBackground()
    {
        _grid.Refresh();
    }

    public void ClearBackground()
    {
        _grid.ClearHighlighting();
    }

    public void ClearNumber()
    {
        _grid.ClearNumbers();
    }

    public void SetCellTo(int row, int col, int number, CellColor color)
    {
        _grid.ShowSolution(row, col, number, ColorUtility.ToBrush(color));
    }

    public void SetCellTo(int row, int col, int[] possibilities, CellColor color)
    {
        foreach (var p in possibilities)
        {
            _grid.ShowGridPossibility(row, col, p, ColorUtility.ToBrush(color));
        }
    }

    public void PutCursorOn(Cell cell)
    {
        _grid.PutCursorOn(cell.Row, cell.Column);
    }

    public void ClearCursor()
    {
        _grid.ClearCursor();
    }
    
    public void FillPossibility(int row, int col, int possibility, ChangeColoration coloration)
    {
        _grid.FillPossibility(row, col, possibility, ColorUtility.ToColor(coloration));
    }

    public void FillCell(int row, int col, ChangeColoration coloration)
    {
        _grid.FillCell(row, col, ColorUtility.ToColor(coloration));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        _grid.EncirclePossibility(row, col, possibility);
    }

    public void EncircleCell(int row, int col)
    {
        _grid.EncircleCell(row, col);
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, ChangeColoration color)
    {
        _grid.EncircleRectangle(rowFrom, colFrom, possibilityFrom, rowTo, colTo, possibilityTo, ColorUtility.ToColor(color));
    }
    
    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, ChangeColoration color)
    {
        _grid.EncircleRectangle(rowFrom, colFrom, rowTo, colTo, ColorUtility.ToColor(color));
    }

    public void EncircleCellPatch(Cell[] cells, ChangeColoration coloration)
    {
        _grid.EncircleCellPatch(cells, ColorUtility.ToColor(coloration));
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        _grid.CreateLink(rowFrom, colFrom, possibilityFrom, rowTo, colTo, possibilityTo,
            strength == LinkStrength.Weak, priority);
    }

    private void KeyPressed(object? sender, KeyEventArgs args)
    {
        switch (args.Key)
        {
            case Key.D1 :
            case Key.NumPad1 : 
                CurrentCellChangeAsked?.Invoke(1);
                break;
            case Key.D2 :
            case Key.NumPad2 : 
                CurrentCellChangeAsked?.Invoke(2);
                break;
            case Key.D3 :
            case Key.NumPad3 : 
                CurrentCellChangeAsked?.Invoke(3);
                break;
            case Key.D4 :
            case Key.NumPad4 : 
                CurrentCellChangeAsked?.Invoke(4);
                break;
            case Key.D5 :
            case Key.NumPad5 : 
                CurrentCellChangeAsked?.Invoke(5);
                break;
            case Key.D6 :
            case Key.NumPad6 : 
                CurrentCellChangeAsked?.Invoke(6);
                break;
            case Key.D7 :
            case Key.NumPad7 : 
                CurrentCellChangeAsked?.Invoke(7);
                break;
            case Key.D8 :
            case Key.NumPad8 : 
                CurrentCellChangeAsked?.Invoke(8);
                break;
            case Key.D9:
            case Key.NumPad9 : 
                CurrentCellChangeAsked?.Invoke(9);
                break;
            case Key.D0 :
            case Key.NumPad0 :
            case Key.Back :
                RemoveSolutionFromCurrentCellAsked?.Invoke();
                break;
        }
    }

    public void TakeScreenShot(Stream stream)
    {
        try
        {
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(_grid.AsImage());
            png.Save(stream);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}

