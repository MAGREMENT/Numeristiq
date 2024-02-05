using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Model.Sudoku;
using Model.Sudoku.Player;
using Model.Utility;
using Brushes = System.Windows.Media.Brushes;
using FlowDirection = System.Windows.FlowDirection;
using MathUtility = View.Utility.MathUtility;

namespace View.Pages;

public class SudokuGrid : FrameworkElement
{
    private static readonly Brush StrongLinkBrush = Brushes.Indigo;
    private const double LinkOffset = 20;
    private const double CursorWidth = 3;
    
    private readonly int _possibilitySize;
    private readonly int _cellSize;
    private readonly int _smallLineWidth;
    private readonly int _bigLineWidth;
    private readonly double _size;

    private readonly DrawingVisual _visual = new();

    private readonly List<IDrawableComponent>[] Layers =
    {
        new(), new(), new(), new(), new(), new(), new(), new(), new(), new()
    };
    private const int BackgroundIndex = 0;
    private  const int CellsHighlightIndex = 1;
    private  const int PossibilitiesHighlightIndex = 2;
    private  const int CursorIndex = 3;
    private  const int SmallMarginsIndex = 4;
    private  const int BigMarginsIndex = 5;
    private  const int NumbersIndex = 6;
    private  const int EncirclesIndex = 7;
    private  const int LinksIndex = 8;

    private bool _isSelecting;
    private bool _overrideSelection = true;

    public event OnCellSelection? CellSelected;
    public event OnCellSelection? CellAddedToSelection;

    public SudokuGrid()
    {
        
    }

    public SudokuGrid(int possibilitySize, int smallLineWidth, int bigLineWidth)
    {
        Focusable = true;

        Loaded += AddVisualToTree;
        Unloaded += RemoveVisualFromTree;
        
        _possibilitySize = possibilitySize;
        _smallLineWidth = smallLineWidth;
        _bigLineWidth = bigLineWidth;
        _cellSize = _possibilitySize * 3;
        
        _size = ComputeSize();
        Width = _size;
        Height = _size;
        Layers[BackgroundIndex].Add(new FilledRectangleComponent(new Rect(0, 0, _size, _size), Brushes.White));
        
        UpdateMargins();
        Refresh();

        MouseLeftButtonDown += (_, args) =>
        {
            Focus();
            var cell = ComputeSelectedCell(args.GetPosition(this));
            if (cell is not null)
            {
                if(_overrideSelection) CellSelected?.Invoke(cell[0], cell[1]);
                else CellAddedToSelection?.Invoke(cell[0], cell[1]);
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

        KeyDown += AnalyseKeyDown;
        KeyUp += AnalyseKeyUp;
    }

    //DrawingNecessities------------------------------------------------------------------------------------------------
    
    // Provide a required override for the VisualChildrenCount property.
    protected override int VisualChildrenCount => 1;

    // Provide a required override for the GetVisualChild method.
    protected override Visual GetVisualChild(int index)
    {
        return _visual;
    }
    
    private void AddVisualToTree(object sender, RoutedEventArgs e)
    {
        AddVisualChild(_visual);
        AddLogicalChild(_visual);
    }

    private void RemoveVisualFromTree(object sender, RoutedEventArgs e)
    {
        RemoveLogicalChild(_visual);
        RemoveVisualChild(_visual);
    }
    
    //------------------------------------------------------------------------------------------------------------------
    
    public void ClearNumbers()
    {
        Layers[NumbersIndex].Clear();
    }

    public void ClearHighlighting()
    {
        Layers[CellsHighlightIndex].Clear();
        Layers[PossibilitiesHighlightIndex].Clear();
        Layers[EncirclesIndex].Clear();
        Layers[LinksIndex].Clear();
    }

    public void ClearCursor()
    {
       Layers[CursorIndex].Clear();
    }
    
    public void ShowGridPossibility(int row, int col, int possibility, Brush color)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Arial"),  (double)_possibilitySize / 4 * 3, color, 1);
        Layers[NumbersIndex].Add(new TextInRectangleComponent(text, new Rect(GetLeft(col, possibility), GetTop(row, possibility),
            _possibilitySize, _possibilitySize), TextHorizontalAlignment.Center, TextVerticalAlignment.Center));
    }

    public void ShowSolution(int row, int col, int possibility, Brush color)
    {
        var text = new FormattedText(possibility.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Arial"), (double)_cellSize / 4 * 3, color, 1);
        Layers[NumbersIndex].Add(new TextInRectangleComponent(text, new Rect(GetLeft(col), GetTop(row),
            _cellSize, _cellSize), TextHorizontalAlignment.Center, TextVerticalAlignment.Center));
    }

    public void ShowLinePossibilities(int row, int col, int[] possibilities, PossibilitiesLocation location,
        Brush color)
    {
        var builder = new StringBuilder();
        foreach (var p in possibilities) builder.Append(p);
        var text = new FormattedText(builder.ToString(), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface("Arial"), (double)_possibilitySize / 4 * 2, color, 1);
        var ha = location switch
        {
            PossibilitiesLocation.Bottom => TextHorizontalAlignment.Right,
            PossibilitiesLocation.Middle => TextHorizontalAlignment.Center,
            PossibilitiesLocation.Top => TextHorizontalAlignment.Left,
            _ => TextHorizontalAlignment.Center
        };
        var n = location switch
        {
            PossibilitiesLocation.Bottom => 7,
            PossibilitiesLocation.Middle => 4,
            PossibilitiesLocation.Top => 1,
            _ => 3
        };

        Layers[NumbersIndex].Add(new TextInRectangleComponent(text, new Rect(GetLeft(col), GetTop(row, n), _cellSize,
            _possibilitySize), ha, TextVerticalAlignment.Center));
    }
    
    public void Refresh()
    {
        var context = _visual.RenderOpen();

        foreach (var list in Layers)
        {
            foreach (var component in list)
            {
                component.Draw(context);
            }
        }
        
        context.Close();
        InvalidateVisual();
    }
    
    public void FillCell(int row, int col, Color color)
    {
        Layers[CellsHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col), GetTop(row),
                _cellSize, _cellSize), new SolidColorBrush(color)));
    }
    
    public void FillCell(int row, int col, double startAngle, int rotationFactor, params Color[] colors)
    {
        if (colors.Length == 0) return;
        if (colors.Length == 1)
        {
            FillCell(row, col, colors[0]);
            return;
        }
        
        var center = Center(row, col);
        var angle = startAngle;
        var angleDelta = 2 * Math.PI / colors.Length;

        var list = Layers[CellsHighlightIndex];
        foreach (var color in colors)
        {
            var next = angle + rotationFactor * angleDelta;
            list.Add(new FilledPolygonComponent(new SolidColorBrush(color),
                MathUtility.GetMultiColorHighlightingPolygon(center, _cellSize, 
                    _cellSize, angle, next, rotationFactor)));
            angle = next;
        }
    }

    public void FillPossibility(int row, int col, int possibility, Color color)
    {
        Layers[PossibilitiesHighlightIndex].Add(new FilledRectangleComponent(new Rect(GetLeft(col, possibility), GetTop(row, possibility),
            _possibilitySize, _possibilitySize), new SolidColorBrush(color)));
    }

    public void EncircleCell(int row, int col)
    {
        var delta = (double)_bigLineWidth / 2;
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(GetLeft(col) - delta, GetTop(row) - delta,
            _cellSize + _bigLineWidth, _cellSize + _bigLineWidth), new Pen(StrongLinkBrush, _bigLineWidth)));
    }
    
    public void EncirclePossibility(int row, int col, int possibility)
    {
        var delta = (double)_smallLineWidth / 2;
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(GetLeft(col, possibility) - delta, GetTop(row, possibility) - delta,
            _possibilitySize + _smallLineWidth, _possibilitySize + _smallLineWidth), new Pen(StrongLinkBrush, _bigLineWidth)));
    }

    public void PutCursorOn(int row, int col)
    {
        ClearCursor();
        
        var delta = CursorWidth / 2;
        var left = GetLeft(col);
        var top = GetTop(row);
        var pen = new Pen(Brushes.MediumPurple, CursorWidth);

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

    public void PutCursorOn(HashSet<Cell> cells)
    {
        ClearCursor();
        
        var delta = CursorWidth / 2;
        var pen = new Pen(Brushes.MediumPurple, CursorWidth);

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
                    new Rect(left, top, CursorWidth, CursorWidth), Brushes.MediumPurple));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top, CursorWidth, CursorWidth), Brushes.MediumPurple));
            }
            
            if(!cells.Contains(new Cell(cell.Row, cell.Column + 1))) list.Add(new LineComponent(
                new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row + 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + _cellSize - delta), new Point(left + _cellSize, top + _cellSize - delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), Brushes.MediumPurple));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), Brushes.MediumPurple));
            }
        }
    }
    
    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, Color color)
    {
        var delta = (double)_smallLineWidth / 2;
        
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
            new Pen(new SolidColorBrush(color), _bigLineWidth)));
    }
    
    public void EncircleRectangle(int rowFrom, int colFrom, int rowTo, int colTo, Color color)
    {
        var delta = (double)_bigLineWidth / 2;
        
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
        
        Layers[EncirclesIndex].Add(new OutlinedRectangleComponent(new Rect(new Point(leftX, topY), new Point(rightX, bottomY)),
            new Pen(new SolidColorBrush(color), _bigLineWidth)));
    }

    public void EncircleCellPatch(Cell[] cells, Color color)
    {
        var delta = (double)_bigLineWidth / 2;
        var brush = new SolidColorBrush(color);
        var pen = new Pen(brush, _bigLineWidth);

        var list = Layers[EncirclesIndex];
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
                    new Rect(left, top, CursorWidth, CursorWidth), Brushes.MediumPurple));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top, CursorWidth, CursorWidth), Brushes.MediumPurple));
            }
            
            if(!cells.Contains(new Cell(cell.Row, cell.Column + 1))) list.Add(new LineComponent(
                new Point(left + _cellSize - delta, top), new Point(left + _cellSize - delta, top + _cellSize), pen));
            
            if(!cells.Contains(new Cell(cell.Row + 1, cell.Column))) list.Add(new LineComponent(
                new Point(left, top + _cellSize - delta), new Point(left + _cellSize, top + _cellSize - delta), pen));
            else
            {
                if(cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), brush));
                
                if(cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) list.Add(new FilledRectangleComponent(
                    new Rect(left + _cellSize - CursorWidth, top + _cellSize - CursorWidth, CursorWidth, CursorWidth), brush));
            }
        }
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo, bool isWeak,
        LinkOffsetSidePriority priority)
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

    public BitmapFrame AsImage()
    {
        var rtb = new RenderTargetBitmap((int)_size, (int)_size, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(_visual);
        return BitmapFrame.Create(rtb);
    }
    
    //Private-----------------------------------------------------------------------------------------------------------

    private double ComputeSize()
    {
        return _cellSize * 9 + _smallLineWidth * 6 + _bigLineWidth * 4;
    }

    private void UpdateMargins()
    {
        var delta = _bigLineWidth + _cellSize;
        for (int i = 0; i < 6; i++)
        {
            Layers[SmallMarginsIndex].Add(new FilledRectangleComponent(new Rect(0, delta, _size, _smallLineWidth), Brushes.Black));
            Layers[SmallMarginsIndex].Add(new FilledRectangleComponent(new Rect(delta, 0, _smallLineWidth, _size), Brushes.Black));

            delta += i % 2 == 0 ? _smallLineWidth + _cellSize : _smallLineWidth + _cellSize + _bigLineWidth + _cellSize;
        }

        delta = 0;
        for (int i = 0; i < 4; i++)
        {
            Layers[BigMarginsIndex].Add(new FilledRectangleComponent(new Rect(0, delta, _size, _bigLineWidth), Brushes.Black));
            Layers[BigMarginsIndex].Add(new FilledRectangleComponent(new Rect(delta, 0, _bigLineWidth, _size), Brushes.Black));

            delta += _cellSize * 3 + _smallLineWidth * 2 + _bigLineWidth;
        }
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
        Layers[LinksIndex].Add(new LineComponent(from, to, new Pen(StrongLinkBrush, 2)
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
        var delta = (double)_possibilitySize / 2;
        return new Point(GetLeft(col, possibility) + delta, GetTop(row, possibility) + delta);
    }

    private Point Center(int row, int col)
    {
        var delta = (double)_cellSize / 2;
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

    private void AnalyseKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.LeftCtrl) _overrideSelection = false;
    }
    
    private void AnalyseKeyUp(object sender, KeyEventArgs args)
    {
        if (args.Key == Key.LeftCtrl) _overrideSelection = true;
    }
}


