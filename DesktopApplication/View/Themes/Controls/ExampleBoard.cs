using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Themes.Controls;

public class ExampleBoard : StackedDrawingBoard, IThemeExampleDrawingData
{
    private const int CellCount = 10;
    
    public override double Size => ActualWidth;
    public override double Space => CellSize / 2;
    public double LineWidth => 3;
    public double CellSize => (Size - (CellCount + 1) * LineWidth) / CellCount;

    public Typeface Typeface { get; } = new(new FontFamily(new Uri("pack://application:,,,/View/Fonts/"), "./#Roboto Mono"),
        FontStyles.Normal, FontWeights.Regular, FontStretches.Normal);
    public CultureInfo CultureInfo { get; } =  CultureInfo.CurrentUICulture;
    
    public Brush LineBrush
    {
        set => SetValue(LineBrushProperty, value);
        get => (Brush)GetValue(LineBrushProperty);
    }

    public ExampleBoard()
    {
        AddLayer(new CellNumbersExampleDrawableComponent());
        //TODO rest
    }
}

public interface IThemeExampleDrawingData : IDefaultSingleSizeConstraintDrawingData
{
    public double LineWidth { get; }
    public double CellSize { get; }
    public Brush LineBrush { get; }
}

public class CellNumbersExampleDrawableComponent : ISingleSizeConstraintDrawableComponent<IThemeExampleDrawingData>
{
    public double Draw(DrawingContext context, double start, IThemeExampleDrawingData data)
    {
        var h = data.CellSize + 2 * data.LineWidth;
        double x = 0;
        for (int i = 0; i < 11; i++)
        {
            context.DrawRectangle(data.LineBrush, null, new Rect(x, start, data.LineWidth, h));
            if (i is > 0 and < 10)
            {
                var text = new FormattedText(i.ToString(), data.CultureInfo, FlowDirection.LeftToRight,
                    data.Typeface, data.CellSize * 3 / 4, data.LineBrush, data.GetPixelsPerDip());
                DrawableComponentHelper.DrawTextInRectangle(context, text, new Rect(x + data.LineWidth,
                        start + data.LineWidth, data.CellSize, data.CellSize),
                    ComponentHorizontalAlignment.Center, ComponentVerticalAlignment.Center);
            }

            x += data.CellSize + data.LineWidth;
        }
        
        context.DrawRectangle(data.LineBrush, null, new Rect(0, start, data.Size, data.LineWidth));
        context.DrawRectangle(data.LineBrush, null, new Rect(0, start + data.CellSize + data.LineWidth,
            data.Size, data.LineWidth));

        return h;
    }
}