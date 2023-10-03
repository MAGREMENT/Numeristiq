using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Model.Solver.StrategiesUtil;

namespace SudokuSolver.Utils;

public class SolverBackgroundManager
{
    private readonly Brush _strongLinkBrush = Brushes.Indigo;
    private const double LinkOffset = 20;

    public int Size { get; }
    public int CellSize { get; }
    private readonly double _oneThird;
    public int Margin { get; }

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
        CellSize = cellSize;
        _oneThird = (double)cellSize / 3;
        Margin = margin;
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
        _cells.Children.Add(GetSquare(TopLeftX(col), TopLeftY(row), CellSize, new SolidColorBrush(color)));
    }

    public void HighlightPossibility(int row, int col, int possibility, Color color)
    {
        _cells.Children.Add(GetSquare(TopLeftX(col, possibility), TopLeftY(row, possibility), _oneThird, new SolidColorBrush(color)));
    }

    public void CirclePossibility(int row, int col, int possibility)
    {
        _groups.Children.Add(new GeometryDrawing
        {
            Geometry = new RectangleGeometry(new Rect(TopLeftX(col, possibility), TopLeftY(row, possibility),
                _oneThird, _oneThird)),
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
            Geometry = new RectangleGeometry(new Rect(TopLeftX(col) - Margin / 2, TopLeftY(row) - Margin / 2,
                CellSize + Margin, CellSize + Margin)),
            Pen = new Pen
            {
                Brush = _strongLinkBrush,
                Thickness = 3.5,
            }
        });
    }
    
    public void HighlightGroup(PointingRow pr, Color color)
    {
        var coords = pr.EachElement();
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
                (CellSize + Margin) * (mostRight.Cell.Col - mostLeft.Cell.Col) + _oneThird, _oneThird)),
            Pen = new Pen()
            {
            Thickness = 3.0,
            Brush = new SolidColorBrush(color),
            DashStyle = DashStyles.DashDot
            }          
        });
    }

    public void HighlightGroup(PointingColumn pc, Color color)
    {
        var coords = pc.EachElement();
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
                TopLeftY(mostUp.Cell.Row, pc.Possibility), _oneThird,
                (CellSize + Margin) * (mostDown.Cell.Row - mostUp.Cell.Row) + _oneThird)),
            Pen = new Pen()
            {
                Thickness = 3.0,
                Brush = new SolidColorBrush(color),
                DashStyle = DashStyles.DashDot
            }          
        });
    }

    public void HighlightGroup(AlmostNakedPossibilities anp, Color color) //TODO take margin into account
    {
        foreach (var coord in anp.CoordinatePossibilities)
        {
            var x = TopLeftX(coord.Cell.Col);
            var y = TopLeftY(coord.Cell.Row);
            
            if(!anp.Contains(coord.Cell.Row - 1, coord.Cell.Col))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y), new Point(x + CellSize, y)),
                    Pen = new Pen()
                    {
                    Thickness = 3.0,
                    Brush = new SolidColorBrush(color),
                    DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row + 1, coord.Cell.Col))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y + CellSize), new Point(x + CellSize, y + CellSize)),
                    Pen = new Pen()
                    {
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row, coord.Cell.Col - 1))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x, y), new Point(x, y + CellSize)),
                    Pen = new Pen()
                    {
                        Thickness = 3.0,
                        Brush = new SolidColorBrush(color),
                        DashStyle = DashStyles.DashDot 
                    }     
                });
            
            if(!anp.Contains(coord.Cell.Row, coord.Cell.Col + 1))
                _groups.Children.Add(new GeometryDrawing()
                {
                    Geometry = new LineGeometry(new Point(x + CellSize, y), new Point(x + CellSize, y + CellSize)),
                    Pen = new Pen()
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
        var from = new Point(one.Col * CellSize + (one.Col + 1) * Margin + (one.Possibility - 1) % 3 * _oneThird + _oneThird / 2,
            one.Row * CellSize + (one.Row + 1) * Margin + (one.Possibility - 1) / 3 * _oneThird + _oneThird / 2);
        var to = new Point(two.Col * CellSize + (two.Col + 1) * Margin + (two.Possibility - 1) % 3 * _oneThird + _oneThird / 2,
            two.Row * CellSize + (two.Row + 1) * Margin + (two.Possibility - 1) / 3 * _oneThird + _oneThird / 2);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        double angle = Math.Atan((to.Y - from.Y) / (to.X - from.X));
        double reverseAngle = Math.PI - angle;

        var deltaX = LinkOffset * Math.Sin(reverseAngle);
        var deltaY = LinkOffset * Math.Cos(reverseAngle);
        var offsetOne = new Point(middle.X + deltaX, middle.Y + deltaY);
        if (offsetOne.X > 0 && offsetOne.X < Size && offsetOne.Y > 0 && offsetOne.Y < Size)
        {
            AddShortenedLine(from, offsetOne, to,  isWeak);
            return;
        }
        
        var offsetTwo = new Point(middle.X - deltaX, middle.Y - deltaY);
        if (offsetTwo.X > 0 && offsetTwo.X < Size && offsetTwo.Y > 0 && offsetTwo.Y < Size)
        {
            AddShortenedLine(from, offsetTwo, to,  isWeak);
            return;
        }

        AddShortenedLine(from, to, isWeak);
    }

    private void AddShortenedLine(Point from, Point to, bool isWeak)
    {
        var space = (double)CellSize / 3;
        var proportion = space / Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        var newFrom = new Point(from.X + proportion * (to.X - from.X), from.Y + proportion * (to.Y - from.Y));
        
        AddLine(newFrom, to, isWeak);
    }
    
    private void AddShortenedLine(Point from, Point middle, Point to, bool isWeak)
    {
        var space = (double)CellSize / 3;
        var proportion = space / Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));
        var newFrom = new Point(from.X + proportion * (middle.X - from.X), from.Y + proportion * (middle.Y - from.Y));
        var newTo = new Point(to.X + proportion * (middle.X - to.X), to.Y + proportion * (middle.Y - to.Y));
        
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
        int startCol = row * CellSize + (row + 1) * Margin;
        int startRow = col * CellSize + (col + 1) * Margin;
        
        int oneFourth = CellSize / 4;
        
        //Top left corner
        _cursor.Children.Add(GetRectangle(startRow - Margin, startCol - Margin, 
            oneFourth, Margin, ColorUtil.Green));
        _cursor.Children.Add(GetRectangle(startRow - Margin, startCol - Margin,
            Margin, oneFourth, ColorUtil.Green));

        //Top right corner
        _cursor.Children.Add(GetRectangle(startRow + CellSize + Margin - oneFourth, startCol - Margin,
            oneFourth, Margin, ColorUtil.Green));
        _cursor.Children.Add(GetRectangle(startRow + CellSize, startCol - Margin,
            Margin, oneFourth, ColorUtil.Green));

        //Bottom left corner
        _cursor.Children.Add(GetRectangle(startRow - Margin, startCol + CellSize,
            oneFourth, Margin, ColorUtil.Green));
        _cursor.Children.Add(GetRectangle(startRow - Margin, startCol + CellSize + Margin - oneFourth,
            Margin, oneFourth, ColorUtil.Green));

        //Bottom right corner
        _cursor.Children.Add(GetRectangle(startRow + CellSize + Margin - oneFourth, startCol + CellSize,
            oneFourth, Margin, ColorUtil.Green));
        _cursor.Children.Add(GetRectangle(startRow + CellSize, startCol + CellSize + Margin - oneFourth,
            Margin, oneFourth, ColorUtil.Green));
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
        return col * CellSize + (col + 1) * Margin;
    }

    private double TopLeftX(int col, int possibility)
    {
        return col * CellSize + (col + 1) * Margin + (possibility - 1) % 3 * _oneThird;
    }

    private double TopLeftY(int row)
    {
        return row * CellSize + (row + 1) * Margin;  
    }

    private double TopLeftY(int row, int possibility)
    {
        return row * CellSize + (row + 1) * Margin + (possibility - 1) / 3 * _oneThird;
    }
}