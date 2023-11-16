using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Global;
using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.AlmostLockedSets;
using Model.Util;

namespace View.Utils;

public class SolverBackgroundManager
{
    private readonly Brush _strongLinkBrush = Brushes.Indigo;
    private const double LinkOffset = 20;

    public double Size { get; }
    
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

    private Cell? _currentCursor;

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

    public void HighlightCell(int row, int col, Color color)
    {
        _cells.Children.Add(GetSquare(TopLeftX(col), TopLeftY(row), _cellSize, new SolidColorBrush(color)));
    }

    public void HighlightPossibility(int row, int col, int possibility, Color color)
    {
        _cells.Children.Add(GetSquare(TopLeftX(col, possibility), TopLeftY(row, possibility), _possibilitySize, new SolidColorBrush(color)));
    }

    public void CirclePossibility(int row, int col, int possibility)
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

    public void CircleCell(int row, int col)
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
    
    public void HighlightGroup(PointingRow pr, Color color)
    {
        var coords = pr.EveryCellPossibilities();
        var mostLeft = coords[0];
        var mostRight = coords[0];
        for (int i = 1; i < coords.Length; i++)
        {
            if (coords[i].Cell.Col < mostLeft.Cell.Col) mostLeft = coords[i];
            if (coords[i].Cell.Col > mostRight.Cell.Col) mostRight = coords[i];
        }

        _groups.Children.Add(new GeometryDrawing()
        {
            Geometry = new RectangleGeometry(new Rect(TopLeftX(mostLeft.Cell.Col, pr.Possibility),
                TopLeftY(mostLeft.Cell.Row, pr.Possibility),
                (_cellSize + _margin) * (mostRight.Cell.Col - mostLeft.Cell.Col) + _possibilitySize, _possibilitySize)),
            Pen = new Pen
            {
            Thickness = 3.0,
            Brush = new SolidColorBrush(color),
            DashStyle = DashStyles.DashDot
            }          
        });
    }

    public void HighlightGroup(PointingColumn pc, Color color)
    {
        var coords = pc.EveryCellPossibilities();
        var mostUp = coords[0];
        var mostDown = coords[0];
        for (int i = 1; i < coords.Length; i++)
        {
            if (coords[i].Cell.Row < mostUp.Cell.Row) mostUp = coords[i];
            if (coords[i].Cell.Row > mostDown.Cell.Row) mostDown = coords[i];
        }

        _groups.Children.Add(new GeometryDrawing()
        {
            Geometry = new RectangleGeometry(new Rect(TopLeftX(mostUp.Cell.Col, pc.Possibility),
                TopLeftY(mostUp.Cell.Row, pc.Possibility), _possibilitySize,
                (_cellSize + _margin) * (mostDown.Cell.Row - mostUp.Cell.Row) + _possibilitySize)),
            Pen = new Pen
            {
                Thickness = 3.0,
                Brush = new SolidColorBrush(color),
                DashStyle = DashStyles.DashDot
            }          
        });
    }

    public void HighlightGroup(AlmostNakedSet anp, Color color) //TODO take margin into account + improve visually
    {
        foreach (var coord in anp.NakedSet)
        {
            var x = TopLeftX(coord.Cell.Col);
            var y = TopLeftY(coord.Cell.Row);
            
            if(!anp.Contains(coord.Cell.Row - 1, coord.Cell.Col))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y), new Point(x + _cellSize, y)),
                    Pen = new Pen
                    {
                    Thickness = 3.0,
                    Brush = new SolidColorBrush(color),
                    DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row + 1, coord.Cell.Col))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y + _cellSize), new Point(x + _cellSize, y + _cellSize)),
                    Pen = new Pen
                    {
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row, coord.Cell.Col - 1))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y), new Point(x, y + _cellSize)),
                    Pen = new Pen
                    {
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row, coord.Cell.Col + 1))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x + _cellSize, y), new Point(x + _cellSize, y + _cellSize)),
                    Pen = new Pen
                    {
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot 
                    }     
                });
        }
    }

    public void CreateLink(CellPossibility one, CellPossibility two, bool isWeak)
    {
        var from = new Point(CenterX(one.Col, one.Possibility), CenterY(one.Row, one.Possibility));
        var to = new Point(CenterX(two.Col, two.Possibility), CenterY(two.Row, two.Possibility));
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtil.ShiftSecondPointPerpendicularly(from.X, from.Y, middle.X, middle.Y, LinkOffset);

        var offsetOne = new Point(offsets[0, 0], offsets[0, 1]);
        if (offsetOne.X > 0 && offsetOne.X < Size && offsetOne.Y > 0 && offsetOne.Y < Size)
        {
            AddShortenedLine(from, offsetOne, to,  isWeak);
            return;
        }
        
        var offsetTwo = new Point(offsets[1, 0], offsets[1, 1]);
        if (offsetTwo.X > 0 && offsetTwo.X < Size && offsetTwo.Y > 0 && offsetTwo.Y < Size)
        {
            AddShortenedLine(from, offsetTwo, to,  isWeak);
            return;
        }

        AddShortenedLine(from, to, isWeak);
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
        
        if (_currentCursor is not null && _currentCursor == new Cell(row, col))
        {
            _currentCursor = null;
            return;
        }

        _currentCursor = new Cell(row, col);
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
    
    private const double PenStrokeWidth = 0.5;

    private GeometryDrawing GetRectangle(double topLeftX, double topLeftY, double width, double height, Brush brush)
    {
        return new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(topLeftX, topLeftY, width, height)),
            Brush = brush,
            Pen = new Pen{
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

    private double CenterY(int row)
    {
        return TopLeftY(row) + _cellSize / 2;
    }

    private double CenterY(int row, int possibility)
    {
        return TopLeftY(row, possibility) + _possibilitySize / 2;
    }
}