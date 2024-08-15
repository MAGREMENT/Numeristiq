using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Utility;
using Model.Core.Changes;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.Collections;

namespace DesktopApplication.View.Tectonics.Controls;

public class TectonicBoard : DrawingBoard, ITectonicDrawingData, ITectonicDrawer, ISizeOptimizable
{
    private const int BackgroundIndex = 0;
    private const int CellHighlightIndex = 1;
    private const int PossibilityHighlightIndex = 2;
    private const int CursorIndex = 3;
    private const int LinesIndex = 4;
    private const int NumbersIndex = 5;
    private const int LinksIndex = 6;

    private double _cellSize;
    private int _rowCount;
    private int _columnCount;
    private double _bigLineWidth;
    private DependantThicknessRange _bigLineRange;
    private double _smallLineWidth;

    private bool[,] _isClue = new bool[0, 0];

    public event OnDimensionCountChange? RowCountChanged;
    public event OnDimensionCountChange? ColumnCountChanged;

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;
    public event OnSelectionEnd? SelectionEnded;
    
    public NotifyingList<NeighborBorder> Borders { get; } = new();
    
    private readonly CellsAssociations _associatedCells;
    private bool _isSelecting;

    public TectonicBoard() : base(7)
    {
        Focusable = true;
        
        _associatedCells = new CellsAssociations(RowCount, ColumnCount);
        Layers[BackgroundIndex].Add(new BackgroundDrawableComponent());
        Layers[LinesIndex].Add(new TectonicGridDrawableComponent());
        
        Borders.Cleared += () =>
        {
            _associatedCells.New(RowCount, ColumnCount);
            Refresh();
        };

        Borders.ElementAdded += e =>
        {
            if (!e.IsThin) return;
            
            var cells = e.ComputeNeighboringCells();
            _associatedCells.Merge(cells.Item1, cells.Item2);
            Refresh();
        };

        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            
            _isSelecting = true;
            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellSelected?.Invoke(pos[0], pos[1]);
        };

        MouseMove += (_, args) =>
        {
            if (!_isSelecting) return;

            var pos = ComputeSelectedCell(args.GetPosition(this));
            if (pos is not null) CellAddedToSelection?.Invoke(pos[0], pos[1]);
        };

        MouseLeftButtonUp += StopSelection;
        MouseLeave += StopSelection;
    }

    #region ITectonicDrawingData
    
    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    public double LinkOffset => 20;
    public LinkOffsetSidePriority LinkOffsetSidePriority { get; set; } = LinkOffsetSidePriority.Any;
    public double InwardCellLineWidth => 3;
    
    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }

    public Brush ClueNumberBrush
    {
        get => (Brush)GetValue(ClueNumberBrushProperty);
        set => SetValue(ClueNumberBrushProperty, value);
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

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
    }

    public Brush CursorBrush
    {
        set => SetValue(CursorBrushProperty, value);
        get => (Brush)GetValue(CursorBrushProperty);
    }

    public double CellSize
    {
        get => _cellSize;
        set => SetCellSize(value, true);
    }

    public int RowCount
    {
        get => _rowCount;
        set
        {
            if (value == _rowCount) return;
            
            _rowCount = value;
            UpdateSize(true);
            AdaptSpecialBrushArray();
            RowCountChanged?.Invoke(_rowCount);
        }
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            if (value == _columnCount) return;
            
            _columnCount = value;
            UpdateSize(true);
            AdaptSpecialBrushArray();
            ColumnCountChanged?.Invoke(_columnCount);
        }
    }

    public double BigLineWidth
    {
        get =>_bigLineWidth;
        private set
        {
            _bigLineWidth = value;
            UpdateSize(true);
        }
    } 

    public DependantThicknessRange BigLineRange
    {
        get => _bigLineRange;
        set
        {
            _bigLineRange = value;
            BigLineWidth = value.GetValueFor(CellSize);
        }
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

    public double GetLeftOfCell(int column)
    {
        return _cellSize * column + _bigLineWidth * (column + 1);
    }

    public double GetTopOfCell(int row)
    {
        return _cellSize * row + _bigLineWidth * (row + 1);
    }

    public double GetLeftOfCellWithBorder(int column)
    {
        return (_cellSize + _bigLineWidth) * column + _bigLineWidth / 2 + _smallLineWidth / 2;
    }
    
    public double GetTopOfCellWithBorder(int row)
    {
        return (_cellSize + _bigLineWidth) * row + _bigLineWidth / 2 + _smallLineWidth / 2;
    }

    public Point GetCenterOfCell(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeftOfCell(col) + delta, GetTopOfCell(row) + delta);
    }

    public bool IsClue(int row, int col)
    {
        return _isClue[row, col];
    }

    public double GetLeftOfPossibility(int row, int column, int possibility)
    {
        var l = GetLeftOfCell(column);
        var size = GetPossibilitySize(row, column);
        switch (_associatedCells.CountAt(row, column))
        {
            case 1 : return l + (_cellSize - size) / 2;
            case 2 : return l + size * (possibility - 1);
            case 3 :
                if(possibility == 1) return l + (_cellSize - size) / 2;
                return l + size * (possibility - 2);
            case 4 : return l + size * ((possibility - 1) % 2);
            case 5 :
                if (possibility is 1 or 4) return l;
                if(possibility is 3) return l + (_cellSize - size) / 2;
                return l + (_cellSize - size);
            default: return 0;
        }
    }

    public double GetTopOfPossibility(int row, int column, int possibility)
    {
        var t = GetTopOfCell(row);
        var size = GetPossibilitySize(row, column);
        switch (_associatedCells.CountAt(row, column))
        {
            case 1 :
            case 2 : return t + (_cellSize - size) / 2;
            case 3 :
                if (possibility == 1) return t;
                return t + size;
            // ReSharper disable once PossibleLossOfFraction
            case 4 : return t + size * ((possibility - 1) / 2);
            case 5 :
                if (possibility is 1 or 2) return t;
                if(possibility is 3) return t + (_cellSize - size) / 2;
                return t + (_cellSize - size);
            default: return 0;
        }
    }

    public Point GetCenterOfPossibility(int row, int col, int possibility)
    {
        var halfSize = GetPossibilitySize(row, col) / 2;

        return new Point(GetLeftOfPossibility(row, col, possibility) + halfSize,
            GetTopOfPossibility(row, col, possibility) + halfSize);
    }

    public double GetPossibilitySize(int row, int col)
    {
        return _associatedCells.CountAt(row, col) switch
        {
            1 => _cellSize / 4 * 3,
            2 => _cellSize / 2,
            3 => _cellSize / 2,
            4 => _cellSize / 2,
            5 => _cellSize / 3,
            _ => 0
        };
    }
    
    public NeighborBorder? GetBorder(BorderDirection direction, int row, int col)
    {
        foreach (var item in Borders)
        {
            if (item is not NeighborBorder nb) continue;
            if (nb.Direction == direction && nb.InsideRow == row && nb.InsideColumn == col) return nb;
        }

        return null;
    }

    #endregion

    #region ITectonicDrawer

    public void ClearNumbers()
    {
        Layers[NumbersIndex].Clear();
    }

    public void ShowSolution(int row, int column, int number)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumbersIndex].Add(new SolutionDrawableComponent(number, row, column));
        });
    }

    public void ShowPossibilities(int row, int column, IEnumerable<int> possibilities)
    {
        Dispatcher.Invoke(() =>
        {
            Layers[NumbersIndex].Add(new VaryingPossibilitiesDrawableComponent(possibilities, row, column));
        });
    }

    public void SetClue(int row, int column, bool isClue)
    {
        _isClue[row, column] = isClue;
    }

    public void ClearBorderDefinitions()
    {
        RefreshAllowed = false;
        Borders.Clear();
        RefreshAllowed = true;
    }

    public void AddBorderDefinition(int insideRow, int insideColumn, BorderDirection direction, bool isThin)
    {
        RefreshAllowed = false;
        Borders.Add(new NeighborBorder(insideRow, insideColumn, direction, isThin));
        RefreshAllowed = true;
    }

    public void PutCursorOn(Cell cell)
    {
        ClearCursor();
        
        Layers[CursorIndex].Add(new InwardCellDrawableComponent(cell.Row, cell.Column, InwardBrushType.Cursor));
    }
    
    public void PutCursorOn(IContainingEnumerable<Cell> cells) //TODO fix
    {
        ClearCursor();

        Layers[CursorIndex].Add(new InwardMultiCellDrawableComponent(cells, InwardBrushType.Cursor));
    }

    public void ClearCursor()
    {
        Layers[CursorIndex].Clear();
    }

    public void ClearHighlights()
    {
        Layers[CellHighlightIndex].Clear();
        Layers[PossibilityHighlightIndex].Clear();
        Layers[LinksIndex].Clear();
    }

    public void FillPossibility(int row, int col, int possibility, StepColor color)
    {
        Layers[PossibilityHighlightIndex].Add(new PossibilityFillDrawableComponent(possibility,
            row, col, (int)color, FillColorType.Step));
    }

    public void FillCell(int row, int col, StepColor color)
    {
        Layers[CellHighlightIndex].Add(new CellFillDrawableComponent(row, col, (int)color, FillColorType.Step));
    }
    
    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        Layers[LinksIndex].Add(new PossibilityLinkDrawableComponent(rowFrom, colFrom, possibilityFrom,
            rowTo, colTo, possibilityTo, strength));
    }

    #endregion

    #region Private
    
    private void SetCellSize(double value, bool fireEvent)
    {
        _cellSize = value;
        _bigLineWidth = BigLineRange.GetValueFor(value);
        UpdateSize(fireEvent);
    }
    
    private void AdaptSpecialBrushArray()
    {
        if (RowCount == 0 || ColumnCount == 0) _isClue = new bool[0, 0];
        else _isClue = new bool[RowCount, ColumnCount];
    }

    private void UpdateSize(bool fireEvent)
    {
        var w = _cellSize * _columnCount + _bigLineWidth * (_columnCount + 1);
        var h = _cellSize * _rowCount + _bigLineWidth * (_rowCount + 1);
        if (Math.Abs(Width - w) < 0.01 && Math.Abs(Height - h) < 0.01) return;

        Width = w;
        Height = h;
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }
    
    private int[]? ComputeSelectedCell(Point point)
    {
        var row = 0;
        var col = 0;

        var y = point.Y;
        var x = point.X;

        for (; row < RowCount; row++)
        {
            if (y < _bigLineWidth) return null;
            y -= _bigLineWidth;
            if (y < _cellSize) break;
            y -= _cellSize;
        }

        for (; col < ColumnCount; col++)
        {
            if (x < _bigLineWidth) return null;
            x -= _bigLineWidth;
            if (x < _cellSize) break;
            x -= _cellSize;
        }

        return row == RowCount || col == ColumnCount ? null : new[] { row, col };
    }

    private void StopSelection(object sender, MouseEventArgs args)
    {
        if (!_isSelecting) return;

        _isSelecting = false;
        SelectionEnded?.Invoke();
    }

    #endregion

    #region ISizeOptimizable

    public event OnSizeChange? OptimizableSizeChanged;
    
    public double GetWidthSizeMetricFor(double space)
    {
        return (space - BigLineRange.Maximum * (_columnCount + 1)) / _columnCount;
    }

    public double GetHeightSizeMetricFor(double space)
    {
        return (space - BigLineRange.Maximum * (_rowCount + 1)) / _rowCount;
    }

    public bool HasSize() => RowCount > 0 && ColumnCount > 0;

    public void SetSizeMetric(double n)
    {
        SetCellSize(n, false);
    }

    #endregion
}

public record NeighborBorder(int InsideRow, int InsideColumn, BorderDirection Direction, bool IsThin)
{
    public (Cell, Cell) ComputeNeighboringCells()
    {
        return Direction == BorderDirection.Horizontal 
            ? (new Cell(InsideRow, InsideColumn), new Cell(InsideRow + 1, InsideColumn)) 
            : (new Cell(InsideRow, InsideColumn), new Cell(InsideRow, InsideColumn + 1));
    }
}

public delegate void OnDimensionCountChange(int number);