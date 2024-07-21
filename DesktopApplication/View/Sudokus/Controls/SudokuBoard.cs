using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.Presenter.Sudokus.Play;
using DesktopApplication.Presenter.Sudokus.Solve;
using DesktopApplication.View.Controls;
using DesktopApplication.View.Utility;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Sudokus.Player;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using MathUtility = DesktopApplication.View.Utility.MathUtility;

namespace DesktopApplication.View.Sudokus.Controls;

public class SudokuBoard : DrawingBoard, ISudokuSolverDrawer, IExplanationHighlighter, ISudokuPlayerDrawer, ISizeOptimizable
{
    private const int BackgroundIndex = 0;
    private const int CellsHighlightIndex = 1;
    private const int PossibilitiesHighlightIndex = 2;
    private const int CursorIndex = 3;
    private const int SmallLinesIndex = 4;
    private const int BigLinesIndex = 5;
    private const int NumbersIndex = 6;
    private const int EncirclesIndex = 7;
    private const int LinksIndex = 8;
    
    private const double LinkOffset = 20;
    private const double CursorWidth = 3;
    private const double PossibilityPadding = 3;
    
    private double _possibilitySize;
    private double _cellSize;
    private double _smallLineWidth;
    private double _bigLineWidth;
    private double _size;

    private readonly bool[,] _isSpecialNumberBrush = new bool[9, 9];

    public static readonly DependencyProperty LinkBrushProperty =
        DependencyProperty.Register(nameof(LinkBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, args) =>
            {
                if (obj is not SudokuBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(LinksIndex, brush);
                board.Refresh();
            }));

    public Brush LinkBrush
    {
        set => SetValue(LinkBrushProperty, value);
        get => (Brush)GetValue(LinkBrushProperty);
    }
    
    public static readonly DependencyProperty DefaultNumberBrushProperty =
        DependencyProperty.Register(nameof(DefaultNumberBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, _) =>
            {
                if (obj is not SudokuBoard board) return;
                board.ReEvaluateNumberBrushes();
                board.Refresh();
            }));

    public Brush DefaultNumberBrush
    {
        set => SetValue(DefaultNumberBrushProperty, value);
        get => (Brush)GetValue(DefaultNumberBrushProperty);
    }
    
    public static readonly DependencyProperty SpecialNumberBrushProperty =
        DependencyProperty.Register(nameof(SpecialNumberBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, _) =>
            {
                if (obj is not SudokuBoard board) return;
                board.ReEvaluateNumberBrushes();
                board.Refresh();
            }));

    public Brush SpecialNumberBrush
    {
        set => SetValue(SpecialNumberBrushProperty, value);
        get => (Brush)GetValue(SpecialNumberBrushProperty);
    }
    
    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, args) =>
            {
                if (obj is not SudokuBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(BackgroundIndex, brush);
                board.Refresh();
            }));

    public Brush BackgroundBrush
    {
        set => SetValue(BackgroundBrushProperty, value);
        get => (Brush)GetValue(BackgroundBrushProperty);
    }
    
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, args) =>
            {
                if (obj is not SudokuBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(SmallLinesIndex, brush);
                board.SetLayerBrush(BigLinesIndex, brush);
                board.Refresh();
            }));
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }
    
    public static readonly DependencyProperty CursorBrushProperty =
        DependencyProperty.Register(nameof(CursorBrush), typeof(Brush), typeof(SudokuBoard),
            new PropertyMetadata((obj, args) =>
            {
                if (obj is not SudokuBoard board || args.NewValue is not Brush brush) return;
                board.SetLayerBrush(CursorIndex, brush);
                board.Refresh();
            }));
    
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

    private void SetCellSize(double value, bool fireEvent)
    {
        _cellSize = value;
        _possibilitySize = _cellSize / 3;
        UpdateSize(fireEvent);
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

    public bool FastPossibilityDisplay { get; set; }
    
    private bool _isSelecting;

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;
    
    public SudokuBoard() : base(9)
    {
        Focusable = true;
        
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

    #region ISudokuDrawer

    public void PutCursorOn(Cell cell)
    {
        ClearCursor();
        
        const double delta = CursorWidth / 2;
        var left = GetLeft(cell.Column);
        var top = GetTop(cell.Row);
        var pen = new Pen(CursorBrush, CursorWidth);

        var list = Layers[CursorIndex];
        list.Add(new LineComponent(new Point(left + delta, top), new Point(left + delta,
            top + _cellSize), pen));
        list.Add(new LineComponent(new Point(left, top + delta), new Point(left + _cellSize,
            top + delta), pen));
        list.Add(new LineComponent(new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta,
            top + _cellSize), pen));
        list.Add(new LineComponent(new Point(left, top + _cellSize - delta), new Point(left + _cellSize,
            top + _cellSize - delta), pen));
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
            var brush = _isSpecialNumberBrush[row, col] ? SpecialNumberBrush : DefaultNumberBrush;
            Layers[NumbersIndex].Add(new SolutionComponent(number.ToString(), _cellSize / 4 * 3, brush,
                new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize), row, col, ComponentHorizontalAlignment.Center,
                ComponentVerticalAlignment.Center));
        });
    }

    #endregion

    #region ISudokuSolverDrawer

    public void ShowPossibilities(int row, int col, IEnumerable<int> possibilities)
    {
        if (FastPossibilityDisplay)
        {
            StringBuilder builder = new();
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    var n = i + j + 1;
                    builder.Append(possibilities.Contains(n) ? (char)('0' + n) : ' ');
                    if (j < 2) builder.Append(' ');
                }

                if (i < 6) builder.Append('\n');
            }
            
            Dispatcher.Invoke(() =>
            {
                Layers[NumbersIndex].Add(new TextInRectangleComponent(builder.ToString(), _cellSize / 10 * 2.5,
                    DefaultNumberBrush, new Rect(GetLeft(col), GetTop(row), _cellSize, _cellSize),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                foreach (var possibility in possibilities)
                {
                    Layers[NumbersIndex].Add(new TextInRectangleComponent(possibility.ToString(), _possibilitySize * 3 / 4,
                        DefaultNumberBrush, new Rect(GetLeft(col, possibility), GetTop(row, possibility), _possibilitySize,
                            _possibilitySize), ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center));
                }
            }); 
        }
    }

    public void SetClue(int row, int column, bool isClue)
    {
        _isSpecialNumberBrush[row, column] = isClue;
    }

    public void FillPossibility(int row, int col, int possibility, StepColor color)
    {
        FillPossibility(row, col, possibility, App.Current.ThemeInformation.ToBrush(color));
    }

    public void FillCell(int row, int col, StepColor color)
    {
        FillCell(row, col, App.Current.ThemeInformation.ToBrush(color));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        var delta = _smallLineWidth / 2;
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(GetLeft(col, possibility) - delta, GetTop(row, possibility) - delta,
            _possibilitySize + _smallLineWidth, _possibilitySize + _smallLineWidth), new Pen(LinkBrush, _bigLineWidth)));
    }

    public void EncircleCell(int row, int col)
    {
        var delta = _bigLineWidth / 2;
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(GetLeft(col) - delta, GetTop(row) - delta,
            _cellSize + _bigLineWidth, _cellSize + _bigLineWidth), new Pen(LinkBrush, _bigLineWidth)));
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        StepColor color)
    {
        var delta = _smallLineWidth / 2;
        
        var xFrom = GetLeft(colFrom, possibilityFrom) - delta;
        var yFrom = GetTop(rowFrom, possibilityFrom) - delta;
        
        var xTo = GetLeft(colTo, possibilityTo) - delta;
        var yTo = GetTop(rowTo, possibilityTo) - delta;

        double leftX, topY, rightX, bottomY;

        if (xFrom < xTo)
        {
            leftX = xFrom;
            rightX = xTo + _possibilitySize + _smallLineWidth;
        }
        else
        {
            leftX = xTo;
            rightX =xFrom + _possibilitySize + _smallLineWidth;
        }

        if (yFrom < yTo)
        {
            topY = yFrom;
            bottomY = yTo + _possibilitySize + _smallLineWidth;
        }
        else
        {
            topY = yTo;
            bottomY = yFrom + _possibilitySize + _smallLineWidth;
        }
        
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(new Point(leftX, topY), new Point(rightX, bottomY)),
            new Pen(App.Current.ThemeInformation.ToBrush(color), _bigLineWidth)));
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, StepColor color)
    {
        EncircleRectangle(rowFrom, colFrom, rowTo, colTo, App.Current.ThemeInformation.ToBrush(color));
    }

    public void DelimitPossibilityPatch(CellPossibility[] cps, StepColor color)
    {
        var w = _possibilitySize / 6;
        var brush = App.Current.ThemeInformation.ToBrush(color);

        var list = Layers[PossibilitiesHighlightIndex];
        foreach (var cp in cps)
        {
            var left = GetLeft(cp.Column, cp.Possibility);
            var top = GetTop(cp.Row, cp.Possibility);

            if(!cps.ContainsAdjacent(cp, 0, -1)) list.Add(new FilledRectangleComponent(
                new Rect(left, top, _possibilitySize, w), brush));
            
            if(!cps.ContainsAdjacent(cp, -1, 0)) list.Add(new FilledRectangleComponent(
                new Rect(left, top, w, _possibilitySize), brush));
            else
            {
                if(cps.ContainsAdjacent(cp, 0, -1) && !cps.ContainsAdjacent(cp, -1, -1)) list.Add(
                    new FilledRectangleComponent(new Rect(left, top, w, w), brush));
                
                if(cps.ContainsAdjacent(cp, 0, 1) && !cps.ContainsAdjacent(cp, -1, 1)) list.Add(
                    new FilledRectangleComponent(new Rect(left, top + _possibilitySize - w,
                        w, w), brush));
            }
            
            if(!cps.ContainsAdjacent(cp, 0, 1)) list.Add(new FilledRectangleComponent(
                new Rect(left, top + _possibilitySize - w, _possibilitySize, w), brush));
            
            if(!cps.ContainsAdjacent(cp, 1, 0)) list.Add(new FilledRectangleComponent(
                new Rect(left + _possibilitySize - w, top, w, _possibilitySize), brush));
            else
            {
                if(cps.ContainsAdjacent(cp, 0, -1) && !cps.ContainsAdjacent(cp, 1, -1)) list.Add(
                    new FilledRectangleComponent(new Rect(left + _possibilitySize - w, top, w, w), brush));
                
                if(cps.ContainsAdjacent(cp, 0, 1) && !cps.ContainsAdjacent(cp, 1, 1)) list.Add(
                    new FilledRectangleComponent(new Rect(left + _possibilitySize - w,
                        top + _possibilitySize - w, w, w), brush));
            }
        }
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo,
        LinkStrength strength, LinkOffsetSidePriority priority)
    {
        var from = Center(rowFrom, colFrom, possibilityFrom);
        var to = Center(rowTo, colTo, possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < _size && p.Y > 0 && p.Y < _size) validOffsets.Add(p);
        }

        bool isWeak = strength == LinkStrength.Weak;
        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(from, to, isWeak);
                break;
            case 1 :
                AddShortenedLine(from, validOffsets[0], to, isWeak);
                break;
            case 2 :
                if(priority == LinkOffsetSidePriority.Any) AddShortenedLine(from, validOffsets[0], to, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    AddShortenedLine(from, priority == LinkOffsetSidePriority.Left 
                        ? validOffsets[left] 
                        : validOffsets[(left + 1) % 2], to, isWeak);
                }
                break;
        }
    }

    #endregion 

    #region ISudokuPlayerDrawer

    public void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors)
    {
        Layers[NumbersIndex].Add(TextComponentForLinePossibilities(row, col, possibilities, location, colors));
    }

    public void ShowLinePossibilities(int row, int col, IEnumerable<int> possibilities, PossibilitiesLocation location,
        IEnumerable<(int, HighlightColor)> colors, int outlinePossibility)
    {
        var component = TextComponentForLinePossibilities(row, col, possibilities, location, colors);
        Layers[NumbersIndex].Add(new OutlinedTextInRectangleComponent(component, CursorBrush, 1,
            (char)('0' + outlinePossibility)));
    }

    public void FillCell(int row, int col, double startAngle, int rotationFactor, params HighlightColor[] colors)
    {
        if (colors.Length == 0) return;
        if (colors.Length == 1)
        {
            Layers[CellsHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col), GetTop(row),
                _cellSize, _cellSize), App.Current.ThemeInformation.ToBrush(colors[0])));
            return;
        }
        
        var center = Center(row, col);
        var angle = startAngle;
        var angleDelta = 2 * Math.PI / colors.Length;

        var list = Layers[CellsHighlightIndex];
        foreach (var color in colors)
        {
            var next = angle + rotationFactor * angleDelta;
            list.Add(new FilledPolygonComponent(App.Current.ThemeInformation.ToBrush(color),
                MathUtility.GetMultiColorHighlightingPolygon(center, _cellSize, 
                    _cellSize, angle, next, rotationFactor)));
            angle = next;
        }
    }
    
    public void PutCursorOn(HashSet<Cell> cells)
    {
        ClearCursor();
        
        var delta = CursorWidth / 2;
        var pen = new Pen(CursorBrush, CursorWidth);

        var list = Layers[CursorIndex];
        foreach (var cell in cells)
        {
            var left = GetLeft(cell.Column);
            var top = GetTop(cell.Row);

            if(!cells.Contains(new Cell(cell.Row, cell.Column - 1))) list.Add(new LineComponent(
                new Point(left + delta, top), new Point(left + delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row - 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + delta), new Point(left + _cellSize, top + delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top, CursorWidth, CursorWidth), CursorBrush));
            }
            
            if(!cells.Contains(new Cell(cell.Row, cell.Column + 1))) list.Add(new LineComponent(
                new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row + 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + _cellSize - delta), new Point(left + _cellSize, top + _cellSize - delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), CursorBrush));
            }
        }
    }
    #endregion
    
    #region IExplanationHighlighter

    public void ShowCell(Cell c, ExplanationColor color)
    {
        FillCell(c.Row, c.Column, App.Current.ThemeInformation.ToBrush(color));
    }

    public void ShowCellPossibility(CellPossibility cp, ExplanationColor color)
    {
        FillPossibility(cp.Row, cp.Column, cp.Possibility, App.Current.ThemeInformation.ToBrush(color));
    }

    public void ShowCoverHouse(House ch, ExplanationColor color)
    {
        var extremities = ch.GetExtremities();
        EncircleRectangle(extremities.Item1.Row, extremities.Item1.Column,
            extremities.Item2.Row, extremities.Item2.Column, App.Current.ThemeInformation.ToBrush(color));
    }

    #endregion

    #region Private
    
    private void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, Brush brush)
    {
        var delta = _bigLineWidth / 2;
        
        var xFrom = GetLeft(colFrom) - delta;
        var yFrom = GetTop(rowFrom) - delta;
        
        var xTo = GetLeft(colTo) - delta;
        var yTo = GetTop(rowTo) - delta;

        double leftX, topY, rightX, bottomY;

        if (xFrom < xTo)
        {
            leftX = xFrom;
            rightX = xTo + _cellSize + _bigLineWidth;
        }
        else
        {
            leftX = xTo;
            rightX = xFrom + _cellSize +  _bigLineWidth;
        }

        if (yFrom < yTo)
        {
            topY = yFrom;
            bottomY = yTo + _cellSize + _bigLineWidth;
        }
        else
        {
            topY = yTo;
            bottomY = yFrom + _cellSize +  _bigLineWidth;
        }
        
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(new Point(leftX, topY),
            new Point(rightX, bottomY)), new Pen(brush, _bigLineWidth)));
    }
    
    private void FillPossibility(int row, int col, int possibility, Brush brush)
    {
        Layers[PossibilitiesHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col, possibility),
            GetTop(row, possibility), _possibilitySize, _possibilitySize), brush));
    }
    
    private void FillCell(int row, int col, Brush brush)
    {
        Layers[CellsHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col), GetTop(row),
            _cellSize, _cellSize), brush));
    }
    
    private TextInRectangleComponent TextComponentForLinePossibilities(int row, int col, IEnumerable<int> possibilities,
        PossibilitiesLocation location, IEnumerable<(int, HighlightColor)> colors)
    {
        var builder = new StringBuilder();
        foreach (var p in possibilities) builder.Append(p);

        var left = GetLeft(col);
        var width = _cellSize;
        ComponentHorizontalAlignment ha;
        int n;

        switch (location)
        {
            case PossibilitiesLocation.Bottom :
                ha = ComponentHorizontalAlignment.Right;
                n = 7;
                width -= PossibilityPadding;
                break;
            case PossibilitiesLocation.Middle :
                ha = ComponentHorizontalAlignment.Center;
                n = 4;
                break;
            case PossibilitiesLocation.Top :
                ha = ComponentHorizontalAlignment.Left;
                n = 1;
                width -= PossibilityPadding;
                left += PossibilityPadding;
                break;
            default:
                ha = ComponentHorizontalAlignment.Center;
                n = 3;
                break;
        }

        var component = new TextInRectangleComponent(builder.ToString(), _possibilitySize / 2, DefaultNumberBrush,
            new Rect(left, GetTop(row, n), width, _possibilitySize), ha, ComponentVerticalAlignment.Center);
        
        foreach (var entry in colors)
        {
            component.SetForegroundFor(App.Current.ThemeInformation.ToBrush(entry.Item2), (char)('0' + entry.Item1));
        }

        return component;
    }

    private void AddShortenedLine(Point from, Point to, bool isWeak)
    {
        var shortening = _possibilitySize / 2;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var mag = Math.Sqrt(dx * dx + dy * dy);
        var newFrom = new Point(from.X + shortening * dx / mag, from.Y + shortening * dy / mag);
        var newTo = new Point(to.X - shortening * dx / mag, to.Y - shortening * dy / mag);
        
        AddLine(newFrom, newTo, isWeak);
    }
    
    private void AddShortenedLine(Point from, Point middle, Point to, bool isWeak)
    {
        var shortening = _possibilitySize / 2;
        
        var dxFrom = middle.X - from.X;
        var dyFrom = middle.Y - from.Y;
        var mag = Math.Sqrt(dxFrom * dxFrom + dyFrom * dyFrom);
        var newFrom = new Point(from.X + shortening * dxFrom / mag, from.Y + shortening * dyFrom / mag);

        var dxTo = to.X - middle.X;
        var dyTo = to.Y - middle.Y;
        mag = Math.Sqrt(dxTo * dxTo + dyTo * dyTo);
        var newTo = new Point(to.X - shortening * dxTo / mag, to.Y - shortening * dyTo / mag);
            
        AddLine(newFrom, middle, isWeak);
        AddLine(middle, newTo, isWeak);
    }

    private void AddLine(Point from, Point to, bool isWeak)
    {
        Layers[LinksIndex].Add(new LineComponent(from, to, new Pen(LinkBrush, 2)
        {
            DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
        }));
    }

    private double GetTop(int row)
    {
        var miniRow = row / 3;
        return row * _cellSize + miniRow * _bigLineWidth + _bigLineWidth + (row - miniRow) * _smallLineWidth;
    }

    private double GetTop(int row, int possibility)
    {
        var miniRow = row / 3;
        var posRow = (possibility - 1) / 3;
        return row * _cellSize + posRow * _possibilitySize + miniRow * _bigLineWidth + _bigLineWidth
               + (row - miniRow) * _smallLineWidth;
    }
    
    private double GetLeft(int col)
    {
        var miniCol = col / 3;
        return col * _cellSize + miniCol * _bigLineWidth + _bigLineWidth + (col - miniCol) * _smallLineWidth;
    }
    
    private double GetLeft(int col, int possibility)
    {
        var miniCol = col / 3;
        var posCol = (possibility - 1) % 3;
        return col * _cellSize + posCol * _possibilitySize + miniCol * _bigLineWidth + _bigLineWidth
               + (col - miniCol) * _smallLineWidth;
    }

    private Point Center(int row, int col, int possibility)
    {
        var delta = _possibilitySize / 2;
        return new Point(GetLeft(col, possibility) + delta, GetTop(row, possibility) + delta);
    }

    private Point Center(int row, int col)
    {
        var delta = _cellSize / 2;
        return new Point(GetLeft(col) + delta, GetTop(row) + delta);
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
    
    private void UpdateSize(bool fireEvent)
    {
        var newSize = _cellSize * 9 + _smallLineWidth * 6 + _bigLineWidth * 4;
        if (Math.Abs(_size - newSize) < 0.01) return;
        
        _size = newSize;
        Width = _size;
        Height = _size;
        
        Clear();
        UpdateBackground();
        UpdateLines();
        Refresh();
        
        if(fireEvent) OptimizableSizeChanged?.Invoke();
    }

    private void UpdateBackground()
    {
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(
            new Rect(0, 0, _size, _size), BackgroundBrush));
    }
    
    private void UpdateLines()
    {
        var delta = _bigLineWidth + _cellSize;
        for (int i = 0; i < 6; i++)
        {
            Layers[SmallLinesIndex].Add(new FilledRectangleComponent(
                new Rect(0, delta, _size, _smallLineWidth), LineBrush));
            Layers[SmallLinesIndex].Add(new FilledRectangleComponent(
                new Rect(delta, 0, _smallLineWidth, _size), LineBrush));

            delta += i % 2 == 0 ? _smallLineWidth + _cellSize : _smallLineWidth + _cellSize + _bigLineWidth + _cellSize;
        }

        delta = 0;
        for (int i = 0; i < 4; i++)
        {
            Layers[BigLinesIndex].Add(new FilledRectangleComponent(
                new Rect(0, delta, _size, _bigLineWidth), LineBrush));
            Layers[BigLinesIndex].Add(new FilledRectangleComponent(
                new Rect(delta, 0, _bigLineWidth, _size), LineBrush));

            delta += _cellSize * 3 + _smallLineWidth * 2 + _bigLineWidth;
        }
    }
    
    private void ReEvaluateNumberBrushes()
    {
        foreach (var component in Layers[NumbersIndex])
        {
            if (component is not SolutionComponent s) continue;
            
            var brush = _isSpecialNumberBrush[s.Row, s.Column] ? SpecialNumberBrush : DefaultNumberBrush;
            component.SetBrush(brush);
        }
    }

    #endregion

    public event OnSizeChange? OptimizableSizeChanged;

    public int WidthSizeMetricCount => 9;
    public int HeightSizeMetricCount => 9;
    public double GetHeightAdditionalSize()
    {
        return _bigLineWidth * 4 + _smallLineWidth * 6;
    }

    public double GetWidthAdditionalSize()
    {
        return _bigLineWidth * 4 + _smallLineWidth * 6;
    }

    public bool HasSize()
    {
        return true;
    }

    public double SimulateSizeMetric(int n, SizeType type)
    {
        return n * 9 + _smallLineWidth * 6 + _bigLineWidth * 4;
    }

    public void SetSizeMetric(int n)
    {
        SetCellSize(n, false);
    }
}