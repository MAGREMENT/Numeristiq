using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Utility;
using Orientation = Model.Utility.Orientation;

namespace DesktopApplication.View.Kakuros.Controls;

public class KakuroBoard : LayeredDrawingBoard, IKakuroDrawingData, ISizeOptimizable, IKakuroSolverDrawer //TODO fix amount cell selection
{
    private const int BackgroundIndex = 0;
    private const int HighlightIndex = 1;
    private const int CursorIndex = 2;
    private const int LineIndex = 3;
    private const int AmountIndex = 4;
    private const int NumberIndex = 5;

    private int _rowCount;
    private int _columnCount;
    private double _cellSize;
    private double _bigLineWidth;
    private double _amountWidthFactor;
    private double _amountHeightFactor;

    private bool[,] _presence = new bool[0, 0];
    private readonly Dictionary<AmountCell, int> _amounts = new();
    
    public double AmountWidthFactor
    {
        get => _amountWidthFactor;
        set
        {
            _amountWidthFactor = value;
            AmountWidth = value * _cellSize;
            UpdateSize(true);
        }
    }
    
    public double AmountHeightFactor
    {
        get => _amountHeightFactor;
        set
        {
            _amountHeightFactor = value;
            AmountHeight = value * _cellSize;
            UpdateSize(true);
        }
    }

    public event OnCellSelection? CellSelected;
    
    public KakuroBoard() : base(6)
    {
        Focusable = true;

        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LineIndex].Add(new KakuroGridDrawableComponent());
        
        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            var result = ComputeSelectedCell(args.GetPosition(this));
            if (result is not null)
            {
                CellSelected?.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3);
            }

            args.Handled = true;
        };
    }

    #region IKakuroDrawingData
    
    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    
    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }

    public Brush ClueNumberBrush => Brushes.Transparent;

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
    
    public static readonly DependencyProperty AmountLineBrushProperty =
        DependencyProperty.Register(nameof(AmountLineBrush), typeof(Brush), typeof(KakuroBoard),
            new PropertyMetadata((obj, _) =>
            {
                if(obj is not KakuroBoard board) return;
                board.Refresh();
            }));
    
    public Brush AmountLineBrush
    {
        set => SetValue(AmountLineBrushProperty, value);
        get => (Brush)GetValue(AmountLineBrushProperty);
    }

    public Brush AmountBrush => (Brush)GetValue(AmountLineBrushProperty);
    
    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
    }

    public Brush LinkBrush => Brushes.Transparent;

    public int RowCount
    {
        get => _rowCount;
        set
        {
            if (value == _rowCount) return;

            _rowCount = value;
            UpdatePresence();
            UpdateSize(true);
        }
    }
    
    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            if (value == _columnCount) return;

            _columnCount = value;
            UpdatePresence();
            UpdateSize(true);
        }
    }

    public double AmountHeight { get; private set; }

    public double AmountWidth { get; private set; }

    public int GetAmount(int row, int col, Orientation orientation)
    {
        return _amounts.TryGetValue(new AmountCell(row, col, orientation), out var v) ? v : -1;
    }

    public IEnumerable<AmountCell> EnumerateAmountCells() => _amounts.Keys;

    public bool IsPresent(int row, int col)
    {
        return _presence[row, col];
    }

    public double InwardCellLineWidth => 5;

    public double CellSize
    {
        get => _cellSize;
        set => SetCellSize(value, true);
    }

    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;

    public double BigLineWidth
    {
        get => _bigLineWidth;
        set
        {
            _bigLineWidth = value;
            UpdateSize(true);
        }
    }

    public double SmallLineWidth => BigLineWidth;

    public double GetLeftOfCell(int column) => 2 * _bigLineWidth + AmountWidth + column * (_bigLineWidth + _cellSize);

    public double GetTopOfCell(int row) => 2 * _bigLineWidth + AmountHeight + row * (_bigLineWidth + _cellSize);
    
    public double GetLeftOfCellWithBorder(int column) => _bigLineWidth + AmountWidth + column * (_bigLineWidth + _cellSize);

    public double GetTopOfCellWithBorder(int row) => _bigLineWidth + AmountHeight + row * (_bigLineWidth + _cellSize);
    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    public bool IsClue(int row, int col) => false;
    
    public bool FastPossibilityDisplay { get; set; }
    public double InwardPossibilityLineWidth => 2;
    public double PossibilityPadding => 2;
    public double GetLeftOfPossibility(int col, int possibility)
    { 
        var posCol = (possibility - 1) % 3;
        return GetLeftOfCell(col) + posCol * CellSize / 3;
    }

    public double GetTopOfPossibility(int row, int possibility)
    {
        var posRow = (possibility - 1) / 3;
        return GetTopOfCell(row) + posRow * CellSize / 3;
    }

    public Point GetCenterOfPossibility(int row, int col, int possibility)
    {
        var delta = CellSize / 6;
        return new Point(GetLeftOfPossibility(col, possibility) + delta, GetTopOfPossibility(row, possibility) + delta);
    }

    #endregion

    public void SetSolution(int row, int col, int n)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumberIndex].Add(new SolutionDrawableComponent(n, row, col));
        });
    }

    public void SetPossibilities(int row, int col, IEnumerable<int> poss)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumberIndex].Add(new NinePossibilitiesDrawableComponent(poss, row, col));
        });
    }

    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
    }

    public void PutCursorOnNumberCell(int row, int col)
    {
        ClearCursor();
        
        Layers[CursorIndex].Add(new InwardCellDrawableComponent(row, col, InwardBrushType.Cursor));
    }

    public void PutCursorOnAmountCell(int row, int col, Orientation orientation)
    {
        ClearCursor();

        Layers[CursorIndex].Add(new InwardAmountCellDrawableComponent(row, col, orientation));
    }

    public void ClearNumbers()
    {
        Dispatcher.Invoke(() => Layers[NumberIndex].Clear());
    }
    
    public void ClearAmounts()
    {
        _amounts.Clear();
        Dispatcher.Invoke(() => Layers[AmountIndex].Clear());
    }

    public void AddAmount(int row, int col, int n, Orientation orientation)
    {
        ReplaceAmount(row, col, n, orientation);

        Layers[AmountIndex].Add(new AmountDrawableComponent(row, col, orientation));
    }
    
    public void ReplaceAmount(int row, int col, int n, Orientation orientation)
    {
        var c = new AmountCell(row, col, orientation);
        _amounts[c] = n;
    }

    public void ClearPresence()
    {
        Array.Clear(_presence);
    }

    public void SetPresence(int row, int col, bool value)
    {
        _presence[row, col] = value;
    }
    
    #region Private
    
    private void SetCellSize(double value, bool fireEvent)
    {
        _cellSize = value;
        AmountWidth = _amountWidthFactor * _cellSize;
        AmountHeight = _amountHeightFactor * _cellSize;
        UpdateSize(fireEvent);
    }

    private void UpdatePresence()
    {
        if (RowCount == _presence.GetLength(0) && ColumnCount == _presence.GetLength(1)) return;
        
        var buffer = new bool[RowCount, ColumnCount];
        for (int row = 0; row < RowCount && row < _presence.GetLength(0); row++)
        {
            for (int col = 0; col < ColumnCount && col < _presence.GetLength(1); col++)
            {
                buffer[row, col] = _presence[row, col];
            }
        }
        _presence = buffer;
    }

    private void UpdateSize(bool fireEvent)
    {
        var w = _columnCount * _cellSize + AmountWidth + (_columnCount + 2) * _bigLineWidth;
        var h = _rowCount * _cellSize + AmountHeight + (_rowCount + 2) * _bigLineWidth;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }
    
    private (Cell, bool, Orientation)? ComputeSelectedCell(Point position)
    {
        var row = -2;
        var col = -2;

        var x = position.X;
        var y = position.Y;

        if (x < _bigLineWidth) return null;
        if (y < _bigLineWidth) return null;

        x -= _bigLineWidth;
        y -= _bigLineWidth;

        if (x < AmountWidth)
        {
            col = -1;
        }

        if (y < AmountHeight)
        {
            row = -1;
        }

        x -= AmountWidth;
        y -= AmountHeight;

        if (col == -2)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                if(x < _bigLineWidth) return null;
                x -= _bigLineWidth;
                if (x < _cellSize)
                {
                    col = c;
                    break;
                }

                x -= _cellSize;
            }
        }

        if (row == -2)
        {
            for (int r = 0; r < RowCount; r++)
            {
                if (y < _bigLineWidth) return null;
                y -= _bigLineWidth;
                if (y < _cellSize)
                {
                    row = r;
                    break;
                }

                y -= _cellSize;
            }
        }

        if (row == -2 || col == -2) return null;

        var cell = new Cell(row, col);
        var ac1 = new AmountCell(row, col, Orientation.Horizontal);
        var ac2 = new AmountCell(row, col, Orientation.Vertical);
        if (_amounts.ContainsKey(ac1) || _amounts.ContainsKey(ac2))
        {
            var objective = GetTopOfCell(row) + position.X - GetLeftOfCell(col);
            return (cell, true, position.Y > objective ? Orientation.Horizontal : Orientation.Vertical);
        }

        if (row >= 0 && col >= 0)
        {
            if (_presence[row, col]) return (cell, false, Orientation.Horizontal);
            if(_amounts.ContainsKey(ac1) || _amounts.ContainsKey(ac2)) return (cell, true, Orientation.Horizontal);
        }

        return null;
    }
    
    #endregion

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - (_columnCount + 2) * _bigLineWidth) / (_columnCount + _amountWidthFactor);
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - (_rowCount + 2) * _bigLineWidth) / (_rowCount + _amountHeightFactor);
    }

    public bool HasSize() => RowCount > 0 && ColumnCount > 0;

    public void SetSizeMetric(double n)
    {
        SetCellSize(n, false);
    }

    #endregion

    public void ClearHighlights()
    {
        Layers[HighlightIndex].Clear();
    }

    public void HighlightPossibility(int possibility, int row, int col, StepColor color)
    {
        Layers[HighlightIndex].Add(new PossibilityFillDrawableComponent(possibility, row, col,
            (int)color, FillColorType.Step));
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        Layers[HighlightIndex].Add(new CellFillDrawableComponent(row, col,
            (int)color, FillColorType.Step));
    }

    public void CreateLink(CellPossibility from, CellPossibility to, LinkStrength linkStrength)
    {
        
    }
}

public readonly struct AmountCell
{
    public AmountCell(int row, int column, Orientation orientation)
    {
        Row = row;
        Column = column;
        Orientation = orientation;
    }

    public int Row { get; }
    public int Column { get; }
    public Orientation Orientation { get; }
}

public delegate void OnCellSelection(Cell cell, bool isAmountCell, Orientation preferred);