using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace SudokuSolver;

public partial class NumbersUserControl
{
    private readonly Grid _small = new();
    private readonly HighlightableTextBlock _big = new();

    public NumbersUserControl()
    {
        InitializeComponent();

        Case = (FindName("Case") as Grid)!;

        _big.TextAlignment = TextAlignment.Center;
        
        _small.RowDefinitions.Add(new RowDefinition());
        _small.RowDefinitions.Add(new RowDefinition());
        _small.RowDefinitions.Add(new RowDefinition());
        _small.ColumnDefinitions.Add(new ColumnDefinition());
        _small.ColumnDefinitions.Add(new ColumnDefinition());
        _small.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                HighlightableTextBlock tb = new HighlightableTextBlock
                {
                    TextAlignment = TextAlignment.Center
                };
                _small.Children.Add(tb);
                Grid.SetRow(tb, i);
                Grid.SetColumn(tb, j);
            }
        }

        _small.VerticalAlignment = VerticalAlignment.Center;
        _small.HorizontalAlignment = HorizontalAlignment.Center;
        _big.VerticalAlignment = VerticalAlignment.Center;
        _big.HorizontalAlignment = HorizontalAlignment.Center;
    }

    public void SetSize(int size)
    {
        Case.Width = size;
        Case.Height = size;
        
        _big.Width = size;
        _big.Height = size;
        _big.FontSize = (double)size * 2 / 3;

        double smallSize = (double)size / 3;
        foreach (var child in _small.Children)
        {
            var tb = (child as HighlightableTextBlock)!;
            tb.Height = smallSize;
            tb.Width = smallSize;
            tb.FontSize = smallSize * 2 / 3;
        }
    }

    public void SetBig(int number)
    {
        Case.Children.Clear();
        Case.Children.Add(_big);
        _big.Text = number.ToString();
    }

    public void SetSmall(int[] numbers)
    {
        Case.Children.Clear();
        Case.Children.Add(_small);
        foreach (var child in _small.Children)
        {
            var tb = (child as HighlightableTextBlock)!;
            int n = Grid.GetRow(tb) * 3 + Grid.GetColumn(tb) + 1;
            if (numbers.Contains(n)) tb.Text = n.ToString();
            else tb.Text = "";
        }
    }

    public void Void()
    {
        Case.Children.Clear();
    }

    public void UnHighLight()
    {
        _big.UnHighlight();
        _small.Background = new SolidColorBrush(Colors.White);
        foreach (var child in _small.Children)
        {
            ((HighlightableTextBlock)child)!.UnHighlight();
        }
    }

    public void HighLightBig(Color color)
    {
        _big.Highlight(color);
        if(Case.Children[0] is Grid) HighLightWholeSmall(color);
    }

    private void HighLightWholeSmall(Color color)
    {
        _small.Background = new SolidColorBrush(color);
        foreach (var child in _small.Children)
        {
            var tb = (child as HighlightableTextBlock)!;
            tb.Highlight(color);
        }
    }

    public void HighLightSmall(int n, Color color)
    {
        foreach (var child in _small.Children)
        {
            var tb = (child as HighlightableTextBlock)!;
            int a = Grid.GetRow(tb) * 3 + Grid.GetColumn(tb) + 1;
            if (a == n) tb.Highlight(color);
        }
    }

    public void Test() //TODO look into
    {
        DrawingBrush brush = new DrawingBrush();

        GeometryDrawing drawing = new()
        {
            Geometry = new RectangleGeometry(new Rect(0, 0, 57, 57)),
            Brush = Brushes.RoyalBlue
        };

        DrawingGroup collection = new();
        collection.Children.Add(drawing);

        brush.Drawing = collection;
    }
}