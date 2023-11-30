using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Global;

namespace View.Utility;

public class SolverBackgroundManager
{
    private readonly Brush _strongLinkBrush = Brushes.Indigo;
    private const double LinkOffset = 20;

    public double Size { get; }

    public LinkOffsetSidePriority SidePriority { get; set; } = LinkOffsetSidePriority.Right;
    
    private readonly double _cellSize;
    private readonly double _possibilitySize;
    private readonly double _margin;

    public Brush Background
    {
        get
        {
            DrawingGroup current = new DrawingGroup();
            current.Children.Add(_cells);
            current.Children.Add(_grid);
            current.Children.Add(_cursor);
            current.Children.Add(_groups);
            current.Children.Add(_links);

            return new DrawingBrush(current);
        }
    }
    
    private readonly DrawingGroup _cells = new();
    private readonly DrawingGroup _grid = new();
    private readonly DrawingGroup _cursor = new();
    private readonly DrawingGroup _groups = new();
    private readonly DrawingGroup _links = new();

    public SolverBackgroundManager(int cellSize, int margin)
    {
        _cellSize = cellSize;
        _possibilitySize = (double)cellSize / 3;
        _margin = margin;
        Size = cellSize * 9 + margin * 10;

        List<GeometryDrawing> after = new();
        int start = 0;
        for (int i = 0; i < 10; i++)
        {
            if (i is 3 or 6)
            {
                after.Add(GetRectangle(start, 0, margin, Size, Brushes.Black));
                after.Add(GetRectangle(0, start, Size, margin, Brushes.Black));
            }
            else
            {
                _grid.Children.Add(GetRectangle(start, 0, margin, Size, Brushes.Gray));
                _grid.Children.Add(GetRectangle(0, start, Size, margin, Brushes.Gray));
            }

            start += margin + cellSize;
        }
        
        foreach (var a in after)
        {
            _grid.Children.Add(a);
        }
    }

    public void Clear()
    {
        _cells.Children.Clear();
        _groups.Children.Clear();
        _links.Children.Clear();
    }

    public void FillCell(int row, int col, Color color)
    {
        _cells.Children.Add(GetSquare(TopLeftX(col), TopLeftY(row), _cellSize, new SolidColorBrush(color)));
    }

    public void FillPossibility(int row, int col, int possibility, Color color)
    {
        _cells.Children.Add(GetSquare(TopLeftX(col, possibility), TopLeftY(row, possibility), _possibilitySize, new SolidColorBrush(color)));
    }

    public void EncirclePossibility(int row, int col, int possibility)
    {
        _groups.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(TopLeftX(col, possibility), TopLeftY(row, possibility),
                _possibilitySize, _possibilitySize)),
            Pen = new Pen
            {
                Brush = _strongLinkBrush,
                Thickness = 3.0,
            }
        });
    }

    public void EncircleCell(int row, int col)
    {
        _groups.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(TopLeftX(col) - _margin / 2, TopLeftY(row) - _margin / 2,
                _cellSize + _margin, _cellSize + _margin)),
            Pen = new Pen
            {
                Brush = _strongLinkBrush,
                Thickness = 3.5,
            }
        });
    }

    public void EncircleRectangle(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo,
        int possibilityTo, Color color)
    {
        var xFrom = CenterX(colFrom, possibilityFrom);
        var yFrom = CenterY(rowFrom, possibilityFrom);
        
        var xTo = CenterX(colTo, possibilityTo);
        var yTo = CenterY(rowTo, possibilityTo);

        double leftX, topY, rightX, bottomY;

        if (xFrom < xTo)
        {
            leftX = TopLeftX(colFrom, possibilityFrom) - _margin / 2;
            rightX = BottomRightX(colTo, possibilityTo) + _margin / 2;
        }
        else
        {
            leftX = TopLeftX(colTo, possibilityTo) - _margin / 2;
            rightX = BottomRightX(colFrom, possibilityFrom) + _margin / 2;
        }

        if (yFrom < yTo)
        {
            topY = TopLeftY(rowFrom, possibilityFrom) - _margin / 2;
            bottomY = BottomRightY(rowTo, possibilityTo) + _margin / 2;
        }
        else
        {
            topY = TopLeftY(rowTo, possibilityTo) - _margin / 2;
            bottomY = BottomRightY(rowFrom, possibilityFrom) + _margin / 2;
        }
        
        _groups.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(new Point(leftX, topY),
                new Point(rightX, bottomY))),
            Pen = new Pen{
                Thickness = 3.0,
                Brush = new SolidColorBrush(color),
                DashStyle = DashStyles.DashDot
            }
        });
    }

    public void EncircleCellPatch(Cell[] cells, Color color)
    {
        foreach (var cell in cells)
        {
            var topLeftX = TopLeftX(cell.Column) - _margin / 2;
            var topLeftY = TopLeftY(cell.Row) - _margin / 2;

            var bottomRightX = topLeftX + _cellSize + _margin;
            var bottomRightY = topLeftY + _cellSize + _margin;

            if (!cells.Contains(new Cell(cell.Row, cell.Column + 1)))
            {
                _groups.Children.Add(new GeometryDrawing
                {
                    Geometry = new LineGeometry(new Point(bottomRightX, topLeftY),
                        new Point(bottomRightX, bottomRightY)),
                    Pen = new Pen{
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot
                    }
                });
            }

            if (!cells.Contains(new Cell(cell.Row, cell.Column - 1)))
            {
                _groups.Children.Add(new GeometryDrawing
                {
                    Geometry = new LineGeometry(new Point(topLeftX, topLeftY),
                        new Point(topLeftX, bottomRightY)),
                    Pen = new Pen{
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot
                    }
                });
            }
            
            if (!cells.Contains(new Cell(cell.Row + 1, cell.Column)))
            {
                _groups.Children.Add(new GeometryDrawing
                {
                    Geometry = new LineGeometry(new Point(topLeftX, bottomRightY),
                        new Point(bottomRightX, bottomRightY)),
                    Pen = new Pen{
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot
                    }
                });
            }

            if (!cells.Contains(new Cell(cell.Row - 1, cell.Column)))
            {
                _groups.Children.Add(new GeometryDrawing
                {
                    Geometry = new LineGeometry(new Point(topLeftX, topLeftY),
                        new Point(bottomRightX, topLeftY)),
                    Pen = new Pen{
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot
                    }
                });
            }
        }
    }

    public void CreateLink(int rowFrom, int colFrom, int possibilityFrom, int rowTo, int colTo, int possibilityTo, bool isWeak)
    {
        var from = new Point(CenterX(colFrom, possibilityFrom), CenterY(rowFrom, possibilityFrom));
        var to = new Point(CenterX(colTo, possibilityTo), CenterY(rowTo, possibilityTo));
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from.X, from.Y, middle.X, middle.Y, LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = new Point(offsets[i, 0], offsets[i, 1]);
            if(p.X > 0 && p.X < Size && p.Y > 0 && p.Y < Size) validOffsets.Add(p);
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
                if(SidePriority == LinkOffsetSidePriority.Any) AddShortenedLine(from, validOffsets[0], to, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    if(SidePriority == LinkOffsetSidePriority.Left) AddShortenedLine(from, validOffsets[left], to, isWeak);
                    else AddShortenedLine(from, validOffsets[(left + 1) % 2], to, isWeak);
                }
                break;
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
        _links.Children.Add(new GeometryDrawing
        {
            Geometry = new LineGeometry(from, to),
            Pen = new Pen
            {
                Thickness = 3.0,
                Brush = _strongLinkBrush,
                DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
            }
        });
    }

    public void PutCursorOn(int row, int col)
    {
        _cursor.Children.Clear();
        
        var startCol = row * _cellSize + (row + 1) * _margin;
        var startRow = col * _cellSize + (col + 1) * _margin;
        
        var oneFourth = _cellSize / 4;

        //Top left corner
        _cursor.Children.Add(GetRectangle(startRow - _margin, startCol - _margin, 
            oneFourth, _margin, ColorManager.Green));
        _cursor.Children.Add(GetRectangle(startRow - _margin, startCol - _margin,
            _margin, oneFourth, ColorManager.Green));

        //Top right corner
        _cursor.Children.Add(GetRectangle(startRow + _cellSize + _margin - oneFourth, startCol - _margin,
            oneFourth, _margin, ColorManager.Green));
        _cursor.Children.Add(GetRectangle(startRow + _cellSize, startCol - _margin,
            _margin, oneFourth, ColorManager.Green));

        //Bottom left corner
        _cursor.Children.Add(GetRectangle(startRow - _margin, startCol + _cellSize,
            oneFourth, _margin, ColorManager.Green));
        _cursor.Children.Add(GetRectangle(startRow - _margin, startCol + _cellSize + _margin - oneFourth,
            _margin, oneFourth, ColorManager.Green));

        //Bottom right corner
        _cursor.Children.Add(GetRectangle(startRow + _cellSize + _margin - oneFourth, startCol + _cellSize,
            oneFourth, _margin, ColorManager.Green));
        _cursor.Children.Add(GetRectangle(startRow + _cellSize, startCol + _cellSize + _margin - oneFourth,
            _margin, oneFourth, ColorManager.Green));
    }

    public void ClearCursor()
    {
        _cursor.Children.Clear();
    }
    
    //Private-----------------------------------------------------------------------------------------------------------
    
    private const double PenStrokeWidth = 0.5;

    private GeometryDrawing GetRectangle(double topLeftX, double topLeftY, double width, double height, Brush brush)
    {
        return new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(topLeftX, topLeftY, width, height)),
            Brush = brush,
            Pen = new Pen {
                Brush = brush,
                Thickness = PenStrokeWidth
            }
        };
    }

    private GeometryDrawing GetSquare(double topLeftX, double topLeftY, double size, Brush brush)
    {
        return GetRectangle(topLeftX, topLeftY, size, size, brush);
    }

    private double TopLeftX(int col)
    {
        return col * _cellSize + (col + 1) * _margin;
    }

    private double TopLeftX(int col, int possibility)
    {
        return col * _cellSize + (col + 1) * _margin + (possibility - 1) % 3 * _possibilitySize;
    }

    private double BottomRightX(int col, int possibility)
    {
        return col * _cellSize + (col + 1) * _margin + (possibility - 1) % 3 * _possibilitySize + _possibilitySize;
    }

    private double CenterX(int col)
    {
        return TopLeftX(col) + _cellSize / 2;
    }

    private double CenterX(int col, int possibility)
    {
        return TopLeftX(col, possibility) + _possibilitySize / 2;
    }

    private double TopLeftY(int row)
    {
        return row * _cellSize + (row + 1) * _margin;  
    }

    private double TopLeftY(int row, int possibility)
    {
        return row * _cellSize + (row + 1) * _margin + (possibility - 1) / 3 * _possibilitySize;
    }
    
    private double BottomRightY(int row, int possibility)
    {
        return row * _cellSize + (row + 1) * _margin + (possibility - 1) / 3 * _possibilitySize + _possibilitySize;
    }

    private double CenterY(int row)
    {
        return TopLeftY(row) + _cellSize / 2;
    }

    private double CenterY(int row, int possibility)
    {
        return TopLeftY(row, possibility) + _possibilitySize / 2;
    }
}

public enum LinkOffsetSidePriority
{
    Any, Left, Right
}