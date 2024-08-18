using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Binairos.Solve;
using DesktopApplication.View.Controls;
using Model.Core.Changes;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.View.Binairos.Controls;

public class BinairoBoard : DrawingBoard, IBinairoDrawingData, ISizeOptimizable, IBinairoDrawer
{
    private const int BackgroundIndex = 0;
    private const int HighlightIndex = 1;
    private const int LinesIndex = 2;
    private const int EncircleIndex = 3;
    private const int NumbersIndex = 4;
    private const int FakeNumbersIndex = 5;
    private const int LinksIndex = 6;

    private double _lineWidth;
    private double _cellSize;
    private int _rowCount;
    private int _columnCount;

    private bool[,] _clues = new bool[0, 0];

    public BinairoBoard() : base(7)
    {
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LinesIndex].Add(new BinairoGridDrawableComponent());
    }

    #region DrawingData

    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    
    public Brush CircleFirstColor 
    {
        set => SetValue(CircleFirstColorProperty, value);
        get => (Brush)GetValue(CircleFirstColorProperty);
    }
    public Brush CircleSecondColor
    {
        set => SetValue(CircleSecondColorProperty, value);
        get => (Brush)GetValue(CircleSecondColorProperty);
    }
    
    public static readonly DependencyProperty CircleFirstColorProperty =
        DependencyProperty.Register(nameof(CircleFirstColor), typeof(Brush), typeof(DrawingBoard),
            new PropertyMetadata((obj, _) =>
    {
        if (obj is not DrawingBoard board) return;
        board.Refresh();
    }));
    
    public static readonly DependencyProperty CircleSecondColorProperty =
        DependencyProperty.Register(nameof(CircleSecondColor), typeof(Brush), typeof(DrawingBoard),
            new PropertyMetadata((obj, _) =>
    {
        if (obj is not DrawingBoard board) return;
        board.Refresh();
    }));

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

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
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
    
    public int RowCount
    {
        get => _rowCount;
        set
        {
            _rowCount = value;
            AdaptClueArray();
            UpdateSize(true);
        }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            _columnCount = value;
            AdaptClueArray();
            UpdateSize(true);
        }
    }

    public double SolutionSimulationSizeFactor => 0.5;

    public double BigLineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize(true);
        }
    }

    public double SmallLineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            UpdateSize(true);
        }
    }

    public double InwardCellLineWidth => 3;

    public double CellSize
    {
        get => _cellSize;
        set
        {
            _cellSize = value;
            UpdateSize(true);
        }
    }

    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;
    public bool AreSolutionNumbers { get; set; } = true;
    public double GetLeftOfCell(int col)
    {
        return _lineWidth + (_lineWidth + _cellSize) * col;
    }

    public double GetLeftOfCellWithBorder(int col)
    {
        return (_lineWidth + _cellSize) * col;
    }

    public double GetTopOfCell(int row)
    {
        return _lineWidth + (_lineWidth + _cellSize) * row;
    }

    public double GetTopOfCellWithBorder(int row)
    {
        return (_lineWidth + _cellSize) * row;
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    public bool IsClue(int row, int col) => _clues[row, col];

    #endregion

    public void ClearSolutions()
    {
        Dispatcher.Invoke(() => Layers[NumbersIndex].Clear());
    }

    public void ShowSolution(int solution, int row, int col)
    {
        Dispatcher.Invoke(() => Layers[NumbersIndex].Add(new BinairoSolutionDrawableComponent(solution, row, col)));
    }

    public void SetClue(int row, int col, bool isClue)
    {
        _clues[row, col] = isClue;
    }

    public void HighlightCell(int row, int col, StepColor color)
    {
        Layers[HighlightIndex].Add(new CellFillDrawableComponent(row, col, (int)color, FillColorType.Step));
    }
    
    public void EncircleCells(IContainingEnumerable<Cell> cells, StepColor color)
    {
        Layers[EncircleIndex].Add(new MultiCellGeometryDrawableComponent(cells, (int)color, FillColorType.Step));
    }

    public void CreateLink(Cell one, Cell two)
    {
        Layers[LinksIndex].Add(new LinkDrawableComponent(one.Row, one.Column,
            two.Row, two.Column));
    }

    public void SimulateSolution(int solution, int row, int col)
    {
        Layers[FakeNumbersIndex].Add(new BinairoSolutionDrawableComponent(solution, row, col, true));
    }

    public void ClearHighlights()
    {
        Layers[HighlightIndex].Clear();
        Layers[EncircleIndex].Clear();
        Layers[LinksIndex].Clear();
        Layers[FakeNumbersIndex].Clear();
    }

    private void AdaptClueArray()
    {
        var buffer = new bool[RowCount, ColumnCount];
        for (int row = 0; row < _clues.GetLength(0) && row < RowCount; row++)
        {
            for (int col = 0; col < _clues.GetLength(1) && col < ColumnCount; col++)
            {
                buffer[row, col] = _clues[row, col];
            }
        }

        _clues = buffer;
    }
    
    private void UpdateSize(bool fireEvent)
    {
        var w = _columnCount * (_lineWidth + _cellSize) + _lineWidth;
        var h = _rowCount * (_lineWidth + _cellSize) + _lineWidth;
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - _lineWidth) / _columnCount - _lineWidth;
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - _lineWidth) / _rowCount - _lineWidth;
    }

    public bool HasSize()
    {
        return _rowCount > 0 && _columnCount > 0;
    }

    public void SetSizeMetric(double n)
    {
        _cellSize = n;
        UpdateSize(false);
    }

    #endregion
}