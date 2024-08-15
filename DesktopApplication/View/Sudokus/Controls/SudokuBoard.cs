using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Core.Graphs;
using Model.Sudokus.Player;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.View.Sudokus.Controls;

public class SudokuBoard : DrawingBoard, ISudokuDrawingData, ISudokuSolverDrawer, IExplanationHighlighter,
    ISudokuPlayerDrawer, ISizeOptimizable
{
    private const int BackgroundIndex = 0;
    private const int CellsHighlightIndex = 1;
    private const int PossibilitiesHighlightIndex = 2;
    private const int CursorIndex = 3;
    private const int LinesIndex = 4;
    private const int NumbersIndex = 5;
    private const int EncirclesIndex = 6;
    private const int LinksIndex = 7;
    
    private double _possibilitySize;
    private double _cellSize;
    private double _smallLineWidth;
    private double _bigLineWidth;

    private readonly bool[,] _isClue = new bool[9, 9];
    
    private bool _isSelecting;

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;
    
    public SudokuBoard() : base(8)
    {
        Focusable = true;
        
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LinesIndex].Add(new SudokuGridDrawableComponent());
        
        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            var cell = ComputeSelectedCell(args.GetPosition(this));
            if (cell is not null)
            {
                if(Keyboard.Modifiers == ModifierKeys.Control) CellAddedToSelection?.Invoke(cell[0], cell[1]);
                else CellSelected?.Invoke(cell[0], cell[1]);
            }

            _isSelecting = true;
        };

        MouseLeftButtonUp += (_, _) => _isSelecting = false;

        MouseMove += (_, args) =>
        {
            if (!_isSelecting) return;
            
            var cell = ComputeSelectedCell(args.GetPosition(this));
            if(cell is not null) CellAddedToSelection?.Invoke(cell[0], cell[1]);
        };
    }

    #region ISudokuDrawingData
    
    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
    }

    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }

    public Brush ClueNumberBrush
    {
        set => SetValue(ClueNumberBrushProperty, value);
        get => (Brush)GetValue(ClueNumberBrushProperty);
    }

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
    }

    public double PossibilitySize
    {
        get => _possibilitySize;
        set
        {
            _possibilitySize = value;
            _cellSize = _possibilitySize * 3;
            UpdateSize(true);
        }
    }

    public double CellSize
    {
        get => _cellSize;
        set => SetCellSize(value, true);
    }
    
    public double SmallLineWidth
    {
        get => _smallLineWidth;
        set
        {
            _smallLineWidth = value;
            UpdateSize(true);
        }
    }
    
    public double BigLineWidth
    {
        get => _bigLineWidth;
        set
        {
            _bigLineWidth = value;
            UpdateSize(true);
        }
    }

    public double LinkOffset => 20;
    public double InwardCellLineWidth => 3;
    public double StartAngle { get; set; } = 45;
    public int RotationFactor { get; set; } = 1;
    public double LinePossibilitiesOutlineWidth => 2;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;
    public bool FastPossibilityDisplay { get; set; }
    public double InwardPossibilityLineWidth => 2;
    public double PossibilityPadding => 3;
    
    public double GetLeftOfCellWithBorder(int col)
    {
        return GetLeftOfCell(col) - col % 3 == 0 ? _bigLineWidth : _smallLineWidth;
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return GetTopOfCell(row) - row % 3 == 0 ? _bigLineWidth : _smallLineWidth;
    }

    public bool IsClue(int row, int col)
    {
        return _isClue[row, col];
    }
    
    public double GetLeftOfPossibility(int col, int possibility)
    {
        var miniCol = col / 3;
        var posCol = (possibility - 1) % 3;
        return col * _cellSize + posCol * _possibilitySize + miniCol * _bigLineWidth + _bigLineWidth
               + (col - miniCol) * _smallLineWidth;
    }
    
    public double GetTopOfCell(int row)
    {
        var miniRow = row / 3;
        return row * _cellSize + miniRow * _bigLineWidth + _bigLineWidth + (row - miniRow) * _smallLineWidth;
    }

    public double GetTopOfPossibility(int row, int possibility)
    {
        var miniRow = row / 3;
        var posRow = (possibility - 1) / 3;
        return row * _cellSize + posRow * _possibilitySize + miniRow * _bigLineWidth + _bigLineWidth
               + (row - miniRow) * _smallLineWidth;
    }
    
    public double GetLeftOfCell(int col)
    {
        var miniCol = col / 3;
        return col * _cellSize + miniCol * _bigLineWidth + _bigLineWidth + (col - miniCol) * _smallLineWidth;
    }

    public Point GetCenterOfPossibility(int row, int col, int possibility)
    {
        var delta = _possibilitySize / 2;
        return new Point(GetLeftOfPossibility(col, possibility) + delta, GetTopOfPossibility(row, possibility) + delta);
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    #endregion

    #region ISudokuDrawer

    public void PutCursorOn(Cell cell)
    {
        ClearCursor();
        Layers[CursorIndex].Add(new InwardCellDrawableComponent(cell.Row, cell.Column, InwardBrushType.Cursor));
    }
    
    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
    }

    public void ClearNumbers()
    {
        Layers[NumbersIndex].Clear();
    }

    public void ClearHighlights()
    {
        Layers[CellsHighlightIndex].Clear();
        Layers[PossibilitiesHighlightIndex].Clear();
        Layers[EncirclesIndex].Clear();
        Layers[LinksIndex].Clear();
    }

    public void ShowSolution(int row, int col, int number)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumbersIndex].Add(new SolutionDrawableComponent(number, row, col));
        });
    }

    #endregion

    #region ISudokuSolverDrawer

    public void ShowPossibilities(int row, int col, IEnumerable<int> possibilities)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumbersIndex].Add(new NinePossibilitiesDrawableComponent(possibilities, row, col));
        });
    }

    public void SetClue(int row, int column, bool isClue)
    {
        _isClue[row, column] = isClue;
    }

    public void FillPossibility(int row, int col, int possibility, StepColor color)
    {
        Layers[PossibilitiesHighlightIndex].Add(new PossibilityFillDrawableComponent(
            possibility, row, col, (int)color, FillColorType.Step));
    }

    public void FillCell(int row, int col, StepColor color)
    {
        Layers[CellsHighlightIndex].Add(new CellFillDrawableComponent(row, col, (int)color,
            FillColorType.Step));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        Layers[EncirclesIndex].Add(new InwardPossibilityDrawableComponent(possibility, row, col, InwardBrushType.Link));
    }

    public void EncircleCell(int row, int col)
    {
        Layers[EncirclesIndex].Add(new InwardCellDrawableComponent(row, col, InwardBrushType.Link));
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        StepColor color)
    {
        Layers[EncirclesIndex].Add(new PossibilityRectangleDrawableComponent(rowFrom, colFrom, possibilityFrom,
            rowTo, colTo, possibilityTo, color));
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, StepColor color)
    {
        Layers[EncirclesIndex].Add(new CellRectangleDrawableComponent(rowFrom, colFrom,
            rowTo, colTo, (int)color, FillColorType.Step));
    }

    public void DelimitPossibilityPatch(CellPossibility[] cps, StepColor color)
    {
        Layers[PossibilitiesHighlightIndex].Add(new PossibilityPatchDrawableComponent(cps, color));
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength)
    {
        Layers[LinksIndex].Add(new PossibilityLinkDrawableComponent(rowFrom, colFrom, possibilityFrom,
            rowTo, colTo, possibilityTo, strength));
    }

    #endregion 

    #region ISudokuPlayerDrawer

    public void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors)
    {
        Layers[NumbersIndex].Add(new LinePossibilitiesDrawableComponent(possibilities,
            row, col, location, colors));
    }

    public void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors, int outlinePossibility)
    {
        Layers[NumbersIndex].Add(new LinePossibilitiesDrawableComponent(possibilities,
            row, col, location, colors, outlinePossibility));
    }

    public void FillCell(int row, int col, params HighlightColor[] colors)
    {
        Layers[CellsHighlightIndex].Add(new MultiColorDrawableComponent(row, col, colors));
    }
    
    public void PutCursorOn(IContainingEnumerable<Cell> cells)
    {
        ClearCursor();
        
        Layers[CursorIndex].Add(new InwardMultiCellDrawableComponent(cells, InwardBrushType.Cursor));
    }
    #endregion
    
    #region IExplanationHighlighter

    public void ShowCell(Cell c, ExplanationColor color)
    {
        Layers[CellsHighlightIndex].Add(new CellFillDrawableComponent(c.Row, c.Column, (int)color,
            FillColorType.Explanation));
    }

    public void ShowCellPossibility(CellPossibility cp, ExplanationColor color)
    {
        Layers[PossibilitiesHighlightIndex].Add(new PossibilityFillDrawableComponent(cp.Possibility,
            cp.Row, cp.Column, (int)color, FillColorType.Explanation));
    }

    public void ShowCoverHouse(House ch, ExplanationColor color)
    {
        var (c1, c2) = ch.GetExtremities();
        Layers[EncirclesIndex].Add(new CellRectangleDrawableComponent(c1.Row, c1.Column,
            c2.Row, c2.Column, (int)color, FillColorType.Explanation));
    }

    #endregion

    #region Private

    private void UpdateSize(bool fireEvent)
    {
        var newSize = _cellSize * 9 + _smallLineWidth * 6 + _bigLineWidth * 4;
        if (Math.Abs(Width - newSize) < 0.01) return;
        
        Width = newSize;
        Height = newSize;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    private void SetCellSize(double value, bool fireEvent)
    {
        _cellSize = value;
        _possibilitySize = value / 3;
        UpdateSize(fireEvent);
    }
    
    private int[]? ComputeSelectedCell(Point point)
    {
        var row = -1;
        var col = -1;

        var y = point.Y;
        var x = point.X;

        for (int i = 0; i < 9; i++)
        {
            var delta = i % 3 == 0 ? _bigLineWidth : _smallLineWidth;

            if (row == -1)
            {
                if (y < delta) return null;
                y -= delta;
                if (y < _cellSize) row = i;
                y -= _cellSize;
            }

            if (col == -1)
            {
                if (x < delta) return null;
                x -= delta;
                if (x < _cellSize) col = i;
                x -= _cellSize;
            }

            if (row != -1 && col != -1) break;
        }

        return row == -1 || col == -1 ? null : new[] { row, col };
    }

    #endregion
    
    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;

    public double GetWidthSizeMetricFor(double space)
    {
        return (space - _smallLineWidth * 6 - _bigLineWidth * 4) / 9;
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - _smallLineWidth * 6 - _bigLineWidth * 4) / 9;
    }

    public bool HasSize()
    {
        return true;
    }

    public void SetSizeMetric(double n)
    {
        SetCellSize(n, false);
    }
    
    #endregion
}