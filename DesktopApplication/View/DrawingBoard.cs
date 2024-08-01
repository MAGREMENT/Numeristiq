using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopApplication.Presenter.Tectonics.Solve;
using DesktopApplication.View.Kakuros.Controls;
using DesktopApplication.View.Tectonics.Controls;
using DesktopApplication.View.Utility;
using Model.Core.Changes;
using Model.Core.Explanation;
using Model.Sudokus.Player;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.Collections;
using MathUtility = DesktopApplication.View.Utility.MathUtility;
using Point = System.Windows.Point;

namespace DesktopApplication.View;

public abstract class DrawingBoard : FrameworkElement
{
    private readonly DrawingVisual _visual = new();
    private readonly List<IDrawableComponent>[] _layers;

    protected IReadOnlyList<List<IDrawableComponent>> Layers => _layers;
    public bool RefreshAllowed { get; set; } = true;

    protected DrawingBoard(int layerCount)
    {
        Loaded += AddVisualToTree;
        Unloaded += RemoveVisualFromTree;

        _layers = new List<IDrawableComponent>[layerCount];
        for (int i = 0; i < _layers.Length; i++)
        {
            _layers[i] = new List<IDrawableComponent>();
        }
    }

    #region DrawingNecessities

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

    #endregion
    
    public void Refresh()
    {
        Dispatcher.Invoke(() =>
        {
            if (!RefreshAllowed) return;
            
            var context = _visual.RenderOpen();

            foreach (var list in _layers)
            {
                foreach (var component in list)
                {
                    component.Draw(context, this);
                }
            }
            
            context.Close();
            InvalidateVisual();
        });
    }
    
    public BitmapFrame AsImage()
    {
        var rtb = new RenderTargetBitmap((int)Width, (int)Height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(_visual);
        return BitmapFrame.Create(rtb);
    }
}

public interface IDrawableComponent
{
    void Draw(DrawingContext context, object data);
}

public interface IDrawableComponent<in T> : IDrawableComponent
{
    void Draw(DrawingContext context, T data);

    void IDrawableComponent.Draw(DrawingContext context, object data)
    {
        if (data is T t) Draw(context, t);
    }
}

public static class DrawableComponentHelper
{
    public static void DrawTextInRectangle(DrawingContext context, FormattedText _text, Rect rect,
        ComponentHorizontalAlignment ha, ComponentVerticalAlignment va)
    {
        var deltaX = (rect.Width - _text.Width) / 2;
        var deltaY = (rect.Height - _text.Height) / 2;
            
        context.DrawText(_text, new Point(rect.X + deltaX * (int)ha, rect.Y + deltaY * (int)va));
    }

    public static Brush GetBrush(InwardBrushType type, ICellGameDrawingData data)
    {
        return type switch
        {
            InwardBrushType.Cursor => data.CursorBrush,
            InwardBrushType.Link => data.LinkBrush,
            _ => throw new Exception()
        };
    }

    public static Brush GetBrush(FillColorType type, int colorAsInt)
    {
        return type switch
        {
            FillColorType.Step => App.Current.ThemeInformation.ToBrush((StepColor)colorAsInt),
            FillColorType.Explanation => App.Current.ThemeInformation.ToBrush((ExplanationColor)colorAsInt),
            _ => throw new Exception()
        };
    }

    public static (double, double) OrderByAndAddToLast(double one, double two, double toAdd)
    {
        return one < two ? (one, two + toAdd) : (two, one + toAdd);

    }

    public static void DrawPolygon(DrawingContext context, IReadOnlyList<Point> points, Brush brush)
    {
        var segmentCollection = new PathSegmentCollection();
        for (int i = 1; i < points.Count; i++)
        {
            segmentCollection.Add(new LineSegment(points[i], true));
        }
        
        var geometry = new PathGeometry
        {
            Figures = new PathFigureCollection
            {
                new()
                {
                    IsClosed = true,
                    StartPoint = points[0],
                    Segments = segmentCollection
                }
            }
        };

        context.DrawGeometry(brush, null, geometry);
    }
}

public interface IDefaultDrawingData
{
    Brush BackgroundBrush { get; }
    
    double Width { get; }
    double Height { get; }
    
    Typeface Typeface { get; }
    CultureInfo CultureInfo { get; }
}

public interface ICellGameDrawingData : IDefaultDrawingData
{
    Brush CursorBrush { get; }
    Brush LinkBrush { get; }
    Brush LineBrush { get; }
    
    double BigLineWidth { get; }
    double SmallLineWidth { get; }
    double InwardCellLineWidth { get; }
    double CellSize { get; }
    double LinkOffset { get; }
    LinkOffsetSidePriority LinkOffsetSidePriority { get; }
    
    double GetLeftOfCell(int col);
    double GetLeftOfCellWithBorder(int col);
    double GetTopOfCell(int row);
    double GetTopOfCellWithBorder(int row);
    Point GetCenterOfCell(int row, int col);
    bool IsClue(int row, int col);
}

public interface IDichotomousCellGameDrawingData : ICellGameDrawingData
{
    Brush FillingBrush { get; }
    Brush UnavailableBrush { get; }
    
    double FillingShift { get; }
    double UnavailableThickness { get; }
}

public interface INumericCellGameDrawingData : ICellGameDrawingData
{
    Brush DefaultNumberBrush { get; }
    Brush ClueNumberBrush { get; }
}

public interface INinePossibilitiesGameDrawingData : INumericCellGameDrawingData
{
    bool FastPossibilityDisplay { get; }
    double InwardPossibilityLineWidth { get; }
    double PossibilityPadding { get; }
    
    double GetLeftOfPossibility(int col, int possibility);
    double GetTopOfPossibility(int row, int possibility);
    Point GetCenterOfPossibility(int row, int col, int possibility);
}

public interface IVaryingPossibilitiesGameDrawingData : INumericCellGameDrawingData
{
    double GetLeftOfPossibility(int row, int col, int possibility);
    double GetTopOfPossibility(int row, int col, int possibility);
    Point GetCenterOfPossibility(int row, int col, int possibility);
    double GetPossibilitySize(int row, int col);
}

public interface ISudokuDrawingData : INinePossibilitiesGameDrawingData
{
    double StartAngle { get; }
    int RotationFactor { get; }
    double LinePossibilitiesOutlineWidth { get; }
}

public interface ITectonicDrawingData : IVaryingPossibilitiesGameDrawingData
{
    int RowCount { get; }
    int ColumnCount { get; }
    NeighborBorder? GetBorder(BorderDirection direction, int row, int col);
}

public interface IKakuroDrawingData : INinePossibilitiesGameDrawingData
{
    Brush AmountBrush { get; }
    Brush AmountLineBrush { get; }
    
    int RowCount { get; }
    int ColumnCount { get; }
    double AmountHeight { get; }
    double AmountWidth { get; }

    int GetAmount(int row, int col, Orientation orientation);
    IEnumerable<AmountCell> EnumerateAmountCells();
    bool IsPresent(int row, int col);
}

public interface INonogramDrawingData : IDichotomousCellGameDrawingData
{
    int RowCount { get; }
    int ColumnCount { get; }
    int MaxDepth { get; }
    int MaxWideness { get; }

    IReadOnlyList<int> GetRowValues(int row);
    IReadOnlyList<int> GetColumnValues(int col);
}

public class InwardAmountCellDrawableComponent : IDrawableComponent<IKakuroDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly Orientation _orientation;

    public InwardAmountCellDrawableComponent(int row, int col, Orientation orientation)
    {
        _row = row;
        _col = col;
        _orientation = orientation;
    }

    public void Draw(DrawingContext context, IKakuroDrawingData data)
    {
        if (_orientation == Orientation.Vertical)
        {
            var xBr = data.GetLeftOfCell(_col) + data.CellSize - data.InwardCellLineWidth;
            var yBr = _row < 0 
                ? data.AmountHeight + data.BigLineWidth + data.InwardCellLineWidth - data.BigLineWidth
                : data.GetTopOfCell(_row) + data.CellSize + data.InwardCellLineWidth - data.BigLineWidth;
            
            context.DrawLine(new Pen(data.CursorBrush, data.BigLineWidth),
                new Point(xBr, yBr), new Point(data.GetLeftOfCellWithBorder(_col), yBr));
        }
        else
        {
            var xBr = data.GetLeftOfCell(_col) + data.CellSize + data.InwardCellLineWidth - data.BigLineWidth;
            var yBr = data.GetTopOfCell(_row) + data.CellSize - data.InwardCellLineWidth;
            
            context.DrawLine(new Pen(data.CursorBrush, data.BigLineWidth),
                new Point(xBr, yBr), new Point(xBr, data.GetTopOfCellWithBorder(_row)));
        }
    }
}

public class AmountDrawableComponent : IDrawableComponent<IKakuroDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly Orientation _orientation;

    public AmountDrawableComponent(int row, int col, Orientation orientation)
    {
        _row = row;
        _col = col;
        _orientation = orientation;
    }

    public void Draw(DrawingContext context, IKakuroDrawingData data)
    {
        var amount = data.GetAmount(_row, _col, _orientation);
        var text = new FormattedText(amount.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
            data.Typeface, data.AmountHeight * 3 / 4, data.AmountBrush, 1);

        double t, l;
        if (_orientation == Orientation.Vertical)
        {
            t = _row < 0 ? data.BigLineWidth : data.GetTopOfCell(_row) + data.CellSize - data.AmountHeight;
            l = data.GetLeftOfCell(_col);
        }
        else
        {
            t = data.GetTopOfCell(_row);
            l = _col < 0 ? data.BigLineWidth : data.GetLeftOfCell(_col) + data.CellSize - data.AmountWidth;
        }
        
        DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(l, t, data.AmountWidth, data.AmountHeight),
            ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center);
    }
}

public class InwardCellDrawableComponent : IDrawableComponent<ICellGameDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly InwardBrushType _brushType;

    public InwardCellDrawableComponent(int row, int col, InwardBrushType brushType)
    {
        _row = row;
        _col = col;
        _brushType = brushType;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.InwardCellLineWidth / 2;
        var left = data.GetLeftOfCell(_col);
        var top = data.GetTopOfCell(_row);
        var pen = new Pen(DrawableComponentHelper.GetBrush(_brushType, data), data.InwardCellLineWidth);

        context.DrawLine(pen, new Point(left + delta, top), new Point(left + delta,
            top + data.CellSize));
        context.DrawLine(pen, new Point(left, top + delta), new Point(left + data.CellSize,
            top + delta));
        context.DrawLine(pen, new Point(left + data.CellSize - delta, top),
            new Point(left + data.CellSize - delta, top + data.CellSize));
        context.DrawLine(pen, new Point(left, top + data.CellSize - delta), 
            new Point(left + data.CellSize, top + data.CellSize - delta));
    }
}

public class InwardMultiCellDrawableComponent : IDrawableComponent<ICellGameDrawingData>
{
    private readonly IContainingEnumerable<Cell> _cells;
    private readonly InwardBrushType _brushType;

    public InwardMultiCellDrawableComponent(IContainingEnumerable<Cell> cells, InwardBrushType brushType)
    {
        _cells = cells;
        _brushType = brushType;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.InwardCellLineWidth / 2;
        var brush = DrawableComponentHelper.GetBrush(_brushType, data);
        var pen = new Pen(brush, data.InwardCellLineWidth);

        foreach (var cell in _cells)
        {
            var left = data.GetLeftOfCell(cell.Column);
            var top = data.GetTopOfCell(cell.Row);

            if(!_cells.Contains(new Cell(cell.Row, cell.Column - 1))) context.DrawLine(pen,
                new Point(left + delta, top), new Point(left + delta, top + data.CellSize));
            
            if(!_cells.Contains(new Cell(cell.Row - 1, cell.Column))) context.DrawLine(pen,
                new Point(left, top + delta), new Point(left + data.CellSize, top + delta));
            else
            {
                if(_cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !_cells.Contains(
                       new Cell(cell.Row - 1, cell.Column - 1))) context.DrawRectangle(brush, null,
                    new Rect(left, top, data.InwardCellLineWidth, data.InwardCellLineWidth));
                
                if(_cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !_cells.Contains(
                       new Cell(cell.Row - 1, cell.Column + 1))) context.DrawRectangle(brush, null,
                    new Rect(left + data.CellSize - data.InwardCellLineWidth, top, data.InwardCellLineWidth, data.InwardCellLineWidth));
            }
            
            if(!_cells.Contains(new Cell(cell.Row, cell.Column + 1))) context.DrawLine(pen,
                new Point(left + data.CellSize - delta, top), new Point(left + data.CellSize - delta, top + data.CellSize));
            
            if(!_cells.Contains(new Cell(cell.Row + 1, cell.Column))) context.DrawLine(pen,
                new Point(left, top + data.CellSize - delta), new Point(left + data.CellSize, top + data.CellSize - delta));
            else
            {
                if(_cells.Contains(new Cell(cell.Row, cell.Column - 1)) && !_cells.Contains(
                       new Cell(cell.Row + 1, cell.Column - 1))) context.DrawRectangle(brush, null,
                    new Rect(left, top + data.CellSize - data.InwardCellLineWidth, data.InwardCellLineWidth, data.InwardCellLineWidth));
                
                if(_cells.Contains(new Cell(cell.Row, cell.Column + 1)) && !_cells.Contains(
                       new Cell(cell.Row + 1, cell.Column + 1))) context.DrawRectangle(brush, null,
                    new Rect(left + data.CellSize - data.InwardCellLineWidth, top + data.CellSize - data.InwardCellLineWidth, data.InwardCellLineWidth, data.InwardCellLineWidth));
            }
        }
    }
}

public class InwardPossibilityDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>
{
    private readonly int _possibility;
    private readonly int _row;
    private readonly int _col;
    private readonly InwardBrushType _brushType;

    public InwardPossibilityDrawableComponent(int possibility, int row, int col, InwardBrushType brushType)
    {
        _possibility = possibility;
        _row = row;
        _col = col;
        _brushType = brushType;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var pSize = data.CellSize / 3;
        var delta = data.InwardPossibilityLineWidth / 2;
        var left = data.GetLeftOfPossibility(_col, _possibility);
        var top = data.GetTopOfPossibility(_row, _possibility);
        var pen = new Pen(DrawableComponentHelper.GetBrush(_brushType, data), data.InwardPossibilityLineWidth);

        context.DrawLine(pen, new Point(left + delta, top), new Point(left + delta,
            top + pSize));
        context.DrawLine(pen, new Point(left, top + delta), new Point(left + pSize,
            top + delta));
        context.DrawLine(pen, new Point(left + pSize - delta, top),
            new Point(left + pSize - delta, top + pSize));
        context.DrawLine(pen, new Point(left, top + pSize - delta), 
            new Point(left + pSize, top + pSize - delta));
    }
}

public enum InwardBrushType
{
    Cursor, Link
}

public class SolutionDrawableComponent : IDrawableComponent<INumericCellGameDrawingData>, IDrawableComponent<IDichotomousCellGameDrawingData>
{
    private readonly int _solution;
    private readonly int _row;
    private readonly int _col;

    public SolutionDrawableComponent(int solution, int row, int col)
    {
        _solution = solution;
        _row = row;
        _col = col;
    }

    public SolutionDrawableComponent(int row, int col) : this(-1, row, col)
    {
        
    }

    public void Draw(DrawingContext context, INumericCellGameDrawingData data)
    {
        var brush = data.IsClue(_row, _col) ? data.ClueNumberBrush : data.DefaultNumberBrush;
        var text = new FormattedText(_solution.ToString(), data.CultureInfo, FlowDirection.LeftToRight, data.Typeface,
            data.CellSize / 4 * 3, brush, 1);
        DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfCell(_col),
            data.GetTopOfCell(_row), data.CellSize, data.CellSize), ComponentHorizontalAlignment.Center,
            ComponentVerticalAlignment.Center);
    }

    public void Draw(DrawingContext context, IDichotomousCellGameDrawingData data)
    {
        var size = data.CellSize - data.FillingShift * 2;
        context.DrawRectangle(data.FillingBrush, null,
            new Rect(data.GetLeftOfCell(_col) + data.FillingShift, data.GetTopOfCell(_row) + data.FillingShift,
                size, size));
    }

    public void Draw(DrawingContext context, object data)
    {
        switch (data)
        {
            case INumericCellGameDrawingData nc : Draw(context, nc);
                break;
            case IDichotomousCellGameDrawingData dc : Draw(context, dc);
                break;
        }
    }
}

public class UnavailabilityDrawableComponent : IDrawableComponent<IDichotomousCellGameDrawingData>
{
    private readonly int _row;
    private readonly int _col;

    public UnavailabilityDrawableComponent( int row, int col)
    {
        _col = col;
        _row = row;
    }

    public void Draw(DrawingContext context, IDichotomousCellGameDrawingData data)
    {
        var shift = data.CellSize / 4;
        var minX = data.GetLeftOfCell(_col) + shift;
        var maxX = data.GetLeftOfCell(_col) + data.CellSize - shift;
        var minY = data.GetTopOfCell(_row) + shift;
        var maxY = data.GetTopOfCell(_row) + data.CellSize - shift;

        var pen = new Pen(data.UnavailableBrush, data.UnavailableThickness);
        context.DrawLine(pen, new Point(minX, minY), new Point(maxX, maxY));
        context.DrawLine(pen, new Point(minX, maxY), new Point(maxX, minY));
    }
}

public class NinePossibilitiesDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>
{
    private readonly IEnumerable<int> _possibilities;
    private readonly int _row;
    private readonly int _col;

    public NinePossibilitiesDrawableComponent(IEnumerable<int> possibilities, int row, int col)
    {
        _possibilities = possibilities;
        _row = row;
        _col = col;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        if (data.FastPossibilityDisplay)
        {
            StringBuilder builder = new();
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    var n = i + j + 1;
                    builder.Append(_possibilities.Contains(n) ? (char)('0' + n) : ' ');
                    if (j < 2) builder.Append(' ');
                }

                if (i < 6) builder.Append('\n');
            }
            
            var text = new FormattedText(builder.ToString(), data.CultureInfo, FlowDirection.LeftToRight, data.Typeface,
                data.CellSize / 4, data.DefaultNumberBrush, 1);
            DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfCell(_col), 
                data.GetTopOfCell(_row), data.CellSize, data.CellSize), ComponentHorizontalAlignment.Center,
                ComponentVerticalAlignment.Center);
        }
        else
        {
            foreach (var possibility in _possibilities)
            {
                var text = new FormattedText(possibility.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                    data.Typeface, data.CellSize / 4, data.DefaultNumberBrush, 1);
                var pSize = data.CellSize / 3;
                DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfPossibility(_col, possibility),
                    data.GetTopOfPossibility(_row, possibility), pSize, pSize), ComponentHorizontalAlignment.Center,
                    ComponentVerticalAlignment.Center);
            }
        }
    }
}

public class VaryingPossibilitiesDrawableComponent : IDrawableComponent<IVaryingPossibilitiesGameDrawingData>
{
    private readonly IEnumerable<int> _possibilities;
    private readonly int _row;
    private readonly int _col;

    public VaryingPossibilitiesDrawableComponent(IEnumerable<int> possibilities, int row, int col)
    {
        _possibilities = possibilities;
        _row = row;
        _col = col;
    }

    public void Draw(DrawingContext context, IVaryingPossibilitiesGameDrawingData data)
    {
        var posSize = data.GetPossibilitySize(_row, _col);
        var textSize = posSize * 3 / 4;
        
        foreach (var possibility in _possibilities)
        {
            var brush = data.IsClue(_row, _col) ? data.ClueNumberBrush : data.DefaultNumberBrush;
            var text = new FormattedText(possibility.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                data.Typeface, textSize, brush, 1);
            DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfPossibility(_row, _col,
                possibility), data.GetTopOfPossibility(_row, _col, possibility), posSize, posSize),
                ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center);
        }
    }
}

public class CellRectangleDrawableComponent : IDrawableComponent<ICellGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly int _colorAsInt;
    private readonly FillColorType _colorType;

    public CellRectangleDrawableComponent(int rowFrom, int colFrom, int rowTo, int colTo, int colorAsInt, FillColorType colorType)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _colorAsInt = colorAsInt;
        _colorType = colorType;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.InwardCellLineWidth / 2;
        
        var xFrom = data.GetLeftOfCell(_colFrom) - delta;
        var yFrom = data.GetTopOfCell(_rowFrom) - delta;
        
        var xTo = data.GetLeftOfCell(_colTo) - delta;
        var yTo = data.GetTopOfCell(_rowTo) - delta;

        var toAdd = data.CellSize + data.InwardCellLineWidth;
        var (leftX, rightX) = DrawableComponentHelper.OrderByAndAddToLast(xFrom, xTo, toAdd);
        var (topY, bottomY) = DrawableComponentHelper.OrderByAndAddToLast(yFrom, yTo, toAdd);
        
        context.DrawRectangle(null, new Pen(DrawableComponentHelper.GetBrush(_colorType, _colorAsInt), data.InwardCellLineWidth),
            new Rect(new Point(leftX, topY), new Point(rightX, bottomY)));
    }
}

public class PossibilityRectangleDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _possibilityFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly int _possibilityTo;
    private readonly StepColor _color;

    public PossibilityRectangleDrawableComponent(int rowFrom, int colFrom, int possibilityFrom, int rowTo,
        int colTo, int possibilityTo, StepColor color)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _possibilityFrom = possibilityFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _possibilityTo = possibilityTo;
        _color = color;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var half = data.InwardPossibilityLineWidth / 2;
        
        var xFrom = data.GetLeftOfPossibility(_colFrom, _possibilityFrom) + half;
        var yFrom = data.GetTopOfPossibility(_rowFrom, _possibilityFrom) + half;
        
        var xTo = data.GetLeftOfPossibility(_colTo, _possibilityTo) + half;
        var yTo = data.GetTopOfPossibility(_rowTo, _possibilityTo) + half;

        var toAdd = data.CellSize / 3 - data.InwardPossibilityLineWidth;
        var (leftX, rightX) = DrawableComponentHelper.OrderByAndAddToLast(xFrom, xTo, toAdd);
        var (topY, bottomY) = DrawableComponentHelper.OrderByAndAddToLast(yFrom, yTo, toAdd);
        
        context.DrawRectangle(null, new Pen(App.Current.ThemeInformation.ToBrush(_color), data.InwardPossibilityLineWidth),
            new Rect(new Point(leftX, topY), new Point(rightX, bottomY)));
    }
}

public class PossibilityPatchDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>
{
    private readonly CellPossibility[] _cps;
    private readonly StepColor _color;

    public PossibilityPatchDrawableComponent(CellPossibility[] cps, StepColor color)
    {
        _cps = cps;
        _color = color;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var brush = App.Current.ThemeInformation.ToBrush(_color);
        var pSize = data.CellSize / 3;
        
        foreach (var cp in _cps)
        {
            var left = data.GetLeftOfPossibility(cp.Column, cp.Possibility);
            var top = data.GetTopOfPossibility(cp.Row, cp.Possibility);

            if(!_cps.ContainsAdjacent(cp, 0, -1)) context.DrawRectangle
                (brush, null, new Rect(left, top, pSize, data.InwardPossibilityLineWidth));
            
            if(!_cps.ContainsAdjacent(cp, -1, 0)) context.DrawRectangle
                (brush, null, new Rect(left, top, data.InwardPossibilityLineWidth, pSize));
            else
            {
                if(_cps.ContainsAdjacent(cp, 0, -1) && !_cps.ContainsAdjacent(cp, -1, -1)) context.DrawRectangle
                    (brush, null, new Rect(left, top, data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
                
                if(_cps.ContainsAdjacent(cp, 0, 1) && !_cps.ContainsAdjacent(cp, -1, 1)) context.DrawRectangle
                    (brush, null, new Rect(left, top + pSize - data.InwardPossibilityLineWidth,
                        data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
            }
            
            if(!_cps.ContainsAdjacent(cp, 0, 1)) context.DrawRectangle
                (brush, null, new Rect(left, top + pSize - data.InwardPossibilityLineWidth, pSize, data.InwardPossibilityLineWidth));
            
            if(!_cps.ContainsAdjacent(cp, 1, 0)) context.DrawRectangle
                (brush, null, new Rect(left + pSize - data.InwardPossibilityLineWidth, top, data.InwardPossibilityLineWidth, pSize));
            else
            {
                if (_cps.ContainsAdjacent(cp, 0, -1) && !_cps.ContainsAdjacent(cp, 1, -1))
                    context.DrawRectangle(brush, null, new Rect(
                        left + pSize - data.InwardPossibilityLineWidth, top, data.InwardPossibilityLineWidth, 
                        data.InwardPossibilityLineWidth));

                if (_cps.ContainsAdjacent(cp, 0, 1) && !_cps.ContainsAdjacent(cp, 1, 1))
                    context.DrawRectangle(brush, null, new Rect(left + pSize - data.InwardPossibilityLineWidth,
                            top + pSize - data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth, data.InwardPossibilityLineWidth));
            }
        }
    }
}

public class PossibilityLinkDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>,
    IDrawableComponent<IVaryingPossibilitiesGameDrawingData>
{
    private readonly int _rowFrom;
    private readonly int _colFrom;
    private readonly int _possibilityFrom;
    private readonly int _rowTo;
    private readonly int _colTo;
    private readonly int _possibilityTo;
    private readonly LinkStrength _link;

    public PossibilityLinkDrawableComponent(int rowFrom, int colFrom, int possibilityFrom, int rowTo, 
        int colTo, int possibilityTo, LinkStrength link)
    {
        _rowFrom = rowFrom;
        _colFrom = colFrom;
        _possibilityFrom = possibilityFrom;
        _rowTo = rowTo;
        _colTo = colTo;
        _possibilityTo = possibilityTo;
        _link = link;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var from = data.GetCenterOfPossibility(_rowFrom, _colFrom, _possibilityFrom);
        var to = data.GetCenterOfPossibility(_rowTo, _colTo, _possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, data.LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < data.Width && p.Y > 0 && p.Y < data.Height) validOffsets.Add(p);
        }

        bool isWeak = _link == LinkStrength.Weak;
        var pSize = data.CellSize / 3;
        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(context, data, from, pSize, to, pSize, isWeak);
                break;
            case 1 :
                AddShortenedLine(context, data, from, pSize, validOffsets[0], to, pSize, isWeak);
                break;
            case 2 :
                if(data.LinkOffsetSidePriority == LinkOffsetSidePriority.Any) 
                    AddShortenedLine(context, data, from, pSize, validOffsets[0], to, pSize, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    AddShortenedLine(context, data, from, pSize, data.LinkOffsetSidePriority == LinkOffsetSidePriority.Left 
                        ? validOffsets[left] 
                        : validOffsets[(left + 1) % 2], to, pSize, isWeak);
                }
                break;
        }
    }
    
    public void Draw(DrawingContext context, IVaryingPossibilitiesGameDrawingData data)
    {
        var from = data.GetCenterOfPossibility(_rowFrom, _colFrom, _possibilityFrom);
        var to = data.GetCenterOfPossibility(_rowTo, _colTo, _possibilityTo);
        var middle = new Point(from.X + (to.X - from.X) / 2, from.Y + (to.Y - from.Y) / 2);

        var offsets = MathUtility.ShiftSecondPointPerpendicularly(from, middle, data.LinkOffset);

        var validOffsets = new List<Point>();
        for (int i = 0; i < 2; i++)
        {
            var p = offsets[i];
            if(p.X > 0 && p.X < data.Width && p.Y > 0 && p.Y < data.Height) validOffsets.Add(p);
        }

        var isWeak = _link == LinkStrength.Weak;
        var fromSize = data.GetPossibilitySize(_rowFrom, _colFrom);
        var toSize = data.GetPossibilitySize(_rowTo, _colTo);
        switch (validOffsets.Count)
        {
            case 0 : 
                AddShortenedLine(context, data, from, fromSize, to, toSize, isWeak);
                break;
            case 1 :
                AddShortenedLine(context, data, from, fromSize, validOffsets[0], to, toSize, isWeak);
                break;
            case 2 :
                if(data.LinkOffsetSidePriority == LinkOffsetSidePriority.Any) 
                    AddShortenedLine(context, data, from, fromSize, validOffsets[0], to, toSize, isWeak);
                else
                {
                    var left = MathUtility.IsLeft(from, to, validOffsets[0]) ? 0 : 1;
                    AddShortenedLine(context, data, from, fromSize,
                        data.LinkOffsetSidePriority == LinkOffsetSidePriority.Left 
                        ? validOffsets[left] 
                        : validOffsets[(left + 1) % 2], to, toSize, isWeak);
                }
                break;
        }
    }
    
    private static void AddShortenedLine(DrawingContext context, ICellGameDrawingData data,
        Point from, double fromSize, Point to, double toSize, bool isWeak)
    {
        var fromShortening = fromSize / 2;
        var toShortening = toSize / 2;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var mag = Math.Sqrt(dx * dx + dy * dy);
        var newFrom = new Point(from.X + fromShortening * dx / mag, from.Y + fromShortening * dy / mag);
        var newTo = new Point(to.X - toShortening * dx / mag, to.Y - toShortening * dy / mag);
        
        AddLine(context, data, newFrom, newTo, isWeak);
    }
    
    private static void AddShortenedLine(DrawingContext context, ICellGameDrawingData data,
        Point from, double fromSize, Point middle, Point to, double toSize, bool isWeak)
    {
        var fromShortening = fromSize / 2;
        var toShortening = toSize / 2;
        
        var dxFrom = middle.X - from.X;
        var dyFrom = middle.Y - from.Y;
        var mag = Math.Sqrt(dxFrom * dxFrom + dyFrom * dyFrom);
        var newFrom = new Point(from.X +fromShortening * dxFrom / mag, from.Y + fromShortening * dyFrom / mag);

        var dxTo = to.X - middle.X;
        var dyTo = to.Y - middle.Y;
        mag = Math.Sqrt(dxTo * dxTo + dyTo * dyTo);
        var newTo = new Point(to.X - toShortening * dxTo / mag, to.Y - toShortening * dyTo / mag);
            
        AddLine(context, data, newFrom, middle, isWeak);
        AddLine(context, data, middle, newTo, isWeak);
    }

    private static void AddLine(DrawingContext context, ICellGameDrawingData data,
        Point from, Point to, bool isWeak)
    {
        context.DrawLine(new Pen(data.LinkBrush, 2)
        {
            DashStyle = isWeak ? DashStyles.DashDot : DashStyles.Solid
        }, from, to);
    }

    public void Draw(DrawingContext context, object data)
    {
        switch (data)
        {
            case IVaryingPossibilitiesGameDrawingData vd : Draw(context, vd);
                break;
            case INinePossibilitiesGameDrawingData nd : Draw(context, nd);
                break;
        }
    }
}

public class CellFillDrawableComponent : IDrawableComponent<ICellGameDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly int _colorAsInt;
    private readonly FillColorType _colorType;

    public CellFillDrawableComponent(int row, int col, int colorAsInt, FillColorType colorType)
    {
        _row = row;
        _col = col;
        _colorAsInt = colorAsInt;
        _colorType = colorType;
    }

    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        context.DrawRectangle(DrawableComponentHelper.GetBrush(_colorType, _colorAsInt), null,
            new Rect(data.GetLeftOfCell(_col), data.GetTopOfCell(_row), data.CellSize, data.CellSize));
    }
}

public class MultiColorDrawableComponent : IDrawableComponent<ISudokuDrawingData>
{
    private readonly int _row;
    private readonly int _col;
    private readonly HighlightColor[] _colors;

    public MultiColorDrawableComponent(int row, int col, HighlightColor[] colors)
    {
        _row = row;
        _col = col;
        _colors = colors;
    }

    public void Draw(DrawingContext context, ISudokuDrawingData data)
    {
        switch (_colors.Length)
        {
            case 0: return;
            case 1:
                context.DrawRectangle(App.Current.ThemeInformation.ToBrush(_colors[0]), null,
                    new Rect(data.GetLeftOfCell(_col), data.GetTopOfCell(_row),
                        data.CellSize, data.CellSize));
                
                return;
            default:
                var center = data.GetCenterOfCell(_row, _col);
                var angle = data.StartAngle;
                var angleDelta = 2 * Math.PI / _colors.Length;

                foreach (var _color in _colors)
                {
                    var next = angle + data.RotationFactor * angleDelta;
                    DrawableComponentHelper.DrawPolygon(context, MathUtility.GetMultiColorHighlightingPolygon(center, data.CellSize, 
                        data.CellSize, angle, next, data.RotationFactor),
                        App.Current.ThemeInformation.ToBrush(_color));
                    angle = next;
                }
                
                break;
        }
    }
}

public class PossibilityFillDrawableComponent : IDrawableComponent<INinePossibilitiesGameDrawingData>,
    IDrawableComponent<IVaryingPossibilitiesGameDrawingData>
{
    private readonly int _possibility;
    private readonly int _row;
    private readonly int _col;
    private readonly int _colorAsInt;
    private readonly FillColorType _colorType;

    public PossibilityFillDrawableComponent(int possibility, int row, int col, int colorAsInt, FillColorType colorType)
    {
        _possibility = possibility;
        _row = row;
        _col = col;
        _colorAsInt = colorAsInt;
        _colorType = colorType;
    }

    public void Draw(DrawingContext context, INinePossibilitiesGameDrawingData data)
    {
        var pSize = data.CellSize / 3;
        context.DrawRectangle(DrawableComponentHelper.GetBrush(_colorType, _colorAsInt), null,
            new Rect(data.GetLeftOfPossibility(_col, _possibility), data.GetTopOfPossibility(_row, _possibility),
                pSize, pSize));
    }

    public void Draw(DrawingContext context, IVaryingPossibilitiesGameDrawingData data)
    {
        var pSize = data.GetPossibilitySize(_row, _col);
        context.DrawRectangle(DrawableComponentHelper.GetBrush(_colorType, _colorAsInt), null,
            new Rect(data.GetLeftOfPossibility(_row, _col, _possibility),
                data.GetTopOfPossibility(_row, _col, _possibility),
                pSize, pSize));
    }

    public void Draw(DrawingContext context, object data)
    {
        switch (data)
        {
            case IVaryingPossibilitiesGameDrawingData vd : Draw(context, vd);
                break;
            case INinePossibilitiesGameDrawingData nd : Draw(context, nd);
                break;
        }
    }
}

public enum FillColorType
{
    Step, Explanation
}

public class BackgroundDrawableComponent : IDrawableComponent<IDefaultDrawingData>
{
    public void Draw(DrawingContext context, IDefaultDrawingData data)
    {
        context.DrawRectangle(data.BackgroundBrush, null, new Rect(0, 0, data.Width, data.Height));
    }
}

public class ValuesHighlightDrawableComponent : IDrawableComponent<INonogramDrawingData>
{
    private readonly int _unit;
    private readonly int _startIndex;
    private readonly int _endIndex;
    private readonly StepColor _color;
    private readonly Orientation _orientation;

    public ValuesHighlightDrawableComponent(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation)
    {
        _unit = unit;
        _startIndex = startIndex;
        _endIndex = endIndex;
        _color = color;
        _orientation = orientation;
    }

    public void Draw(DrawingContext context, INonogramDrawingData data)
    {
        double left, right, top, bottom, hShift, wShift;
        
        if (_orientation == Orientation.Horizontal)
        {
            left = (data.MaxWideness - data.GetRowValues(_unit).Count + _startIndex) * data.CellSize / 2;
            right = left + (_endIndex - _startIndex + 1) * data.CellSize / 2;
            top = data.GetTopOfCell(_unit);
            bottom = top + data.CellSize;

            hShift = data.CellSize / 4;
            wShift = data.CellSize / 10;
        }
        else
        {
            top = (data.MaxDepth - data.GetColumnValues(_unit).Count + _startIndex) * data.CellSize / 2;
            bottom = top + (_endIndex - _startIndex + 1) * data.CellSize / 2;
            left = data.GetLeftOfCell(_unit);
            right = left + data.CellSize;
        
            hShift = data.CellSize / 12;
            wShift = data.CellSize / 3;
        }
        
        context.DrawRectangle(App.Current.ThemeInformation.ToBrush(_color), null,
            new Rect(new Point(left + wShift, top + hShift), 
                new Point(right - wShift, bottom - hShift)));
    }
}

public class CellSectionDrawableComponent : IDrawableComponent<INonogramDrawingData>
{
    private readonly int _unit;
    private readonly int _startIndex;
    private readonly int _endIndex;
    private readonly StepColor _color;
    private readonly Orientation _orientation;

    public CellSectionDrawableComponent(int unit, int startIndex, int endIndex, StepColor color, Orientation orientation)
    {
        _unit = unit;
        _startIndex = startIndex;
        _endIndex = endIndex;
        _color = color;
        _orientation = orientation;
    }

    public void Draw(DrawingContext context, INonogramDrawingData data)
    {
        double left, top, bottom, right;
        if (_orientation == Orientation.Horizontal)
        {
            left = data.GetLeftOfCell(_startIndex) - data.BigLineWidth / 2;
            top = data.GetTopOfCell(_unit) - data.BigLineWidth / 2;
            bottom = top + data.BigLineWidth + data.CellSize;
            right = left + (_endIndex - _startIndex + 1) * (data.BigLineWidth + data.CellSize);
        }
        else
        {
            left = data.GetLeftOfCell(_unit) - data.BigLineWidth / 2;
            top = data.GetTopOfCell(_startIndex) - data.BigLineWidth / 2;
            bottom = top + (_endIndex - _startIndex + 1) * (data.BigLineWidth + data.CellSize);
            right = left + data.BigLineWidth + data.CellSize;
        }

        context.DrawRectangle(null, new Pen(App.Current.ThemeInformation.ToBrush(_color), data.BigLineWidth),
            new Rect(new Point(left, top), new Point(right, bottom)));
    }
}

public class SudokuGridDrawableComponent : IDrawableComponent<ICellGameDrawingData>
{
    public void Draw(DrawingContext context, ICellGameDrawingData data)
    {
        var delta = data.BigLineWidth + data.CellSize;
        for (int i = 0; i < 6; i++)
        {
            context.DrawRectangle(data.LineBrush, null,
                new Rect(0, delta, data.Width, data.SmallLineWidth));
            context.DrawRectangle(data.LineBrush, null,
                new Rect(delta, 0, data.SmallLineWidth, data.Height));

            delta += i % 2 == 0 ? data.SmallLineWidth + data.CellSize : data.SmallLineWidth + data.CellSize 
                + data.BigLineWidth + data.CellSize;
        }

        delta = 0;
        for (int i = 0; i < 4; i++)
        {
            context.DrawRectangle(data.LineBrush, null,
                new Rect(0, delta, data.Width, data.BigLineWidth));
            context.DrawRectangle(data.LineBrush, null,
                new Rect(delta, 0, data.BigLineWidth, data.Height));

            delta += data.CellSize * 3 + data.SmallLineWidth * 2 + data.BigLineWidth;
        }
    }
}

public class TectonicGridDrawableComponent : IDrawableComponent<ITectonicDrawingData>
{
    public void Draw(DrawingContext context, ITectonicDrawingData data)
    {
        if (data.RowCount == 0 || data.ColumnCount == 0) return;
        
        var half = data.BigLineWidth / 2;
        context.DrawRectangle(null, new Pen(data.LineBrush, data.BigLineWidth),
            new Rect(half, half, data.Width - data.BigLineWidth, data.Height - data.BigLineWidth));

        var diff = (data.BigLineWidth - data.SmallLineWidth) / 2;
        var length = data.CellSize + data.BigLineWidth * 2;

        //Horizontal
        double deltaX;
        double deltaY = data.CellSize + data.BigLineWidth;
        
        for (int row = 0; row < data.RowCount - 1; row++)
        {
            deltaX = 0;
            
            for (int col = 0; col < data.ColumnCount; col++)
            {
                var b = data.GetBorder(BorderDirection.Horizontal, row, col);

                if (b is not null && b.IsThin)
                {
                    context.DrawRectangle(data.LineBrush, null,
                        new Rect(deltaX, deltaY + diff, length, data.SmallLineWidth));
                }
                else
                {
                    context.DrawRectangle(data.LineBrush, null,
                        new Rect(deltaX, deltaY, length, data.BigLineWidth));
                }

                deltaX += data.CellSize + data.BigLineWidth;
            }

            deltaY += data.CellSize + data.BigLineWidth;
        }
        
        //Vertical
        deltaY = 0;
        
        for (int row = 0; row < data.RowCount; row++)
        {
            deltaX = data.CellSize + data.BigLineWidth;
            
            for (int col = 0; col < data.ColumnCount - 1; col++)
            {
                var b = data.GetBorder(BorderDirection.Vertical, row, col);

                if (b is not null && b.IsThin)
                {
                    context.DrawRectangle(data.LineBrush, null,
                        new Rect(deltaX + diff, deltaY, data.SmallLineWidth, length));
                }
                else
                {
                    context.DrawRectangle(data.LineBrush, null,
                        new Rect(deltaX, deltaY, data.BigLineWidth, length));
                }

                deltaX += data.CellSize + data.BigLineWidth;
            }

            deltaY += data.CellSize + data.BigLineWidth;
        }
    }
}

public class KakuroGridDrawableComponent : IDrawableComponent<IKakuroDrawingData> //TODO fix amount cell lines diagonal
{ 
    public void Draw(DrawingContext context, IKakuroDrawingData data)
    {
        if (data.RowCount == 0 || data.ColumnCount == 0) return;
        
        var pen = new Pen(data.AmountLineBrush, data.BigLineWidth);
        foreach (var cell in data.EnumerateAmountCells())
        {
            var half = data.BigLineWidth / 2;
            if (cell.Orientation == Orientation.Vertical)
            {
                var xBr = data.GetLeftOfCell(cell.Column) + data.CellSize + half;
                var yBr = cell.Row < 0 
                    ? data.AmountHeight + data.BigLineWidth + half
                    : data.GetTopOfCell(cell.Row) + data.CellSize + half;
                var xTr = xBr - data.AmountHeight;
                var yTr = yBr - data.AmountWidth;
                context.DrawLine(pen, new Point(xBr, yBr), new Point(xTr, yTr));

                var xTl = data.GetLeftOfCell(cell.Column) - half;
                if (cell.Column <= 0 || cell.Row < 0 || !data.IsPresent(cell.Row, cell.Column - 1))
                {
                    xTl -= data.AmountHeight;
                    if (data.GetAmount(cell.Row, cell.Column - 1, Orientation.Vertical) != -1)
                    {
                        var xBl = data.GetLeftOfCell(cell.Column) - half;
                        context.DrawLine(pen, new Point(xTl, yTr), new Point(xBl, yBr));
                    }

                    xTl -= data.BigLineWidth / 2;
                }
                    
                context.DrawLine(pen, new Point(xTl, yTr), new Point(xTr, yTr));
            }
            else
            {
                var xBr = data.GetLeftOfCell(cell.Column) + data.CellSize + half;
                var yBr = data.GetTopOfCell(cell.Row) + data.CellSize + half;
                var xBl = xBr - data.AmountHeight;
                var yBl = yBr - data.AmountHeight;
                context.DrawLine(pen, new Point(xBr, yBr), new Point(xBl, yBl));

                var yTl = data.GetTopOfCell(cell.Row) - half;
                if (cell.Column < 0 || cell.Row <= 0 || !data.IsPresent(cell.Row - 1, cell.Column))
                {
                    yTl -= data.AmountWidth + data.BigLineWidth / 2;
                }
                    
                context.DrawLine(pen, new Point(xBl, yBl), new Point(xBl, yTl));
            }
        }

        var l = data.CellSize + data.BigLineWidth * 2;
        for (int row = 0; row < data.RowCount; row++)
        {
            for (int col = 0; col < data.ColumnCount; col++)
            {
                if (!data.IsPresent(row, col)) continue;
                
                context.DrawRectangle(data.LineBrush, null,
                    new Rect(data.GetLeftOfCell(col) + data.CellSize, data.GetTopOfCellWithBorder(row),
                        data.BigLineWidth, l));
                context.DrawRectangle(data.LineBrush, null,
                    new Rect(data.GetLeftOfCellWithBorder(col), data.GetTopOfCell(row) + data.CellSize,
                        l, data.BigLineWidth));
                
                if(row == 0 || !data.IsPresent(row - 1, col)) context.DrawRectangle(data.LineBrush, null,
                        new Rect(data.GetLeftOfCellWithBorder(col), data.GetTopOfCellWithBorder(row),
                            l, data.BigLineWidth));
                
                if(col == 0 || !data.IsPresent(row, col - 1)) context.DrawRectangle(data.LineBrush, null,
                    new Rect(data.GetLeftOfCellWithBorder(col), data.GetTopOfCellWithBorder(row),
                        data.BigLineWidth, l));
            }
        }
    }
}

public class NonogramGridDrawableComponent : IDrawableComponent<INonogramDrawingData>
{
    public void Draw(DrawingContext context, INonogramDrawingData data)
    {
        if (data.RowCount == 0 || data.ColumnCount == 0) return;
        
        var yStart = data.MaxDepth * data.CellSize / 2;
        var xStart = data.MaxWideness * data.CellSize / 2;

        var currentY = yStart;
        for (int row = 0; row <= data.RowCount; row++)
        {
            context.DrawRectangle(data.LineBrush, null,
                new Rect(xStart, currentY, data.Width - xStart, data.BigLineWidth));

            currentY += data.BigLineWidth + data.CellSize;
        }
        
        var currentX = xStart;
        for (int col = 0; col <= data.ColumnCount; col++)
        {
            context.DrawRectangle(data.LineBrush, null,
                new Rect(currentX, yStart, data.BigLineWidth, data.Height - yStart));
          
            currentX += data.BigLineWidth + data.CellSize;
        }
    }
}

public class NonogramNumbersDrawableComponent : IDrawableComponent<INonogramDrawingData>
{
    public void Draw(DrawingContext context, INonogramDrawingData data)
    {
        var size = data.CellSize / 3;
        
        for (int col = 0; col < data.ColumnCount; col++)
        {
            var current = data.GetColumnValues(col);
            var depth = data.MaxDepth;
            for (int i = current.Count - 1; i >= 0; i--)
            {
                var text = new FormattedText(current[i].ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                    data.Typeface, size, data.LineBrush, 1);
                DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.GetLeftOfCell(col),
                        data.CellSize / 2 * (depth - 1), data.CellSize, data.CellSize / 2),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center);
                
                depth--;
            }
        }

        for (int row = 0; row < data.RowCount; row++)
        {
            var current = data.GetRowValues(row);
            var width = data.MaxWideness;
            for (int i = current.Count - 1; i >= 0; i--)
            {
                var text = new FormattedText(current[i].ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                    data.Typeface, size, data.LineBrush, 1);
                DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(data.CellSize / 2 * (width - 1),
                        data.GetTopOfCell(row), data.CellSize / 2, data.CellSize),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center);
                
                width--;
            }
        }
    }
}

public class LinePossibilitiesDrawableComponent : IDrawableComponent<ISudokuDrawingData>
{
    private readonly IEnumerable<int> _possibilities;
    private readonly int _row;
    private readonly int _col;
    private readonly PossibilitiesLocation _location;
    private readonly IEnumerable<(int, HighlightColor)> _colors;
    private readonly int _outlinedPossibility;

    public LinePossibilitiesDrawableComponent(IEnumerable<int> possibilities, int row, int col,
        PossibilitiesLocation location, IEnumerable<(int, HighlightColor)> colors, int outlinedPossibility = -1)
    {
        _possibilities = possibilities;
        _row = row;
        _col = col;
        _location = location;
        _colors = colors;
        _outlinedPossibility = outlinedPossibility;
    }

    public void Draw(DrawingContext context, ISudokuDrawingData data)
    {
        var builder = new StringBuilder();
        foreach (var p in _possibilities) builder.Append(p);

        var left = data.GetLeftOfCell(_col);
        var width = data.CellSize;
        ComponentHorizontalAlignment ha;
        int n;

        switch (_location)
        {
            case PossibilitiesLocation.Bottom :
                ha = ComponentHorizontalAlignment.Right;
                n = 7;
                width -= data.PossibilityPadding;
                break;
            case PossibilitiesLocation.Middle :
                ha = ComponentHorizontalAlignment.Center;
                n = 4;
                break;
            case PossibilitiesLocation.Top :
                ha = ComponentHorizontalAlignment.Left;
                n = 1;
                width -= data.PossibilityPadding;
                left += data.PossibilityPadding;
                break;
            default:
                ha = ComponentHorizontalAlignment.Center;
                n = 3;
                break;
        }

        var text = new FormattedText(builder.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
            data.Typeface, data.CellSize / 6, data.DefaultNumberBrush, 1);
        
        foreach (var entry in _colors)
        {
            var letter = (char)('0' + entry.Item1);
            for (int i = 0; i < text.Text.Length; i++)
            {
                if(text.Text[i] == letter) text.SetForegroundBrush(
                    App.Current.ThemeInformation.ToBrush(entry.Item2), i, 1);
            }
        }

        var rect = new Rect(left, data.GetTopOfPossibility(_row, n), width, data.CellSize / 3);
        DrawableComponentHelper.DrawTextInRectangle(context, text, rect, ha, ComponentVerticalAlignment.Center);

        if (_outlinedPossibility == -1) return;
        
        var deltaX = (rect.Width - text.Width) / 2;
        var deltaY = (rect.Height - text.Height) / 2;
        
        var point = new Point(rect.X + deltaX * (int)ha,
            rect.Y + deltaY * (int)ComponentVerticalAlignment.Center);
        
        var outlinedLetter = (char)('0' + _outlinedPossibility);
        for (int i = 0; i < text.Text.Length; i++)
        {
            if (text.Text[i] != outlinedLetter) continue;
            
            var geometry = text.BuildHighlightGeometry(point, i, 1);
            context.DrawGeometry(null, new Pen(data.CursorBrush, data.LinePossibilitiesOutlineWidth), geometry);
        }
    }
}

public delegate void OnCellSelection(int row, int col);

public delegate void OnSelectionEnd();

public enum ComponentVerticalAlignment
{
    Top = 0, Center = 1, Bottom = 2
}

public enum ComponentHorizontalAlignment
{
    Left = 0, Center = 1, Right = 2
}