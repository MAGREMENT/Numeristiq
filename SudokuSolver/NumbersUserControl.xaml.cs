using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SudokuSolver;

public partial class NumbersUserControl : UserControl
{
    private readonly Grid _case;

    private readonly Grid _small = new();
    private readonly TextBlock _big = new();

    public NumbersUserControl()
    {
        InitializeComponent();

        _case = (FindName("Case") as Grid)!;

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
                TextBlock tb = new TextBlock
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
        _case.Width = size;
        _case.Height = size;
        
        _big.Width = size;
        _big.Height = size;
        _big.FontSize = (double)size * 2 / 3;

        double smallSize = (double)size / 3;
        foreach (var child in _small.Children)
        {
            var tb = (child as TextBlock)!;
            tb.Height = smallSize;
            tb.Width = smallSize;
            tb.FontSize = smallSize * 2 / 3;
        }
    }

    public void SetBig(int number)
    {
        _case.Children.Clear();
        _case.Children.Add(_big);
        _big.Text = number.ToString();
    }

    public void SetSmall(int[] numbers)
    {
        _case.Children.Clear();
        _case.Children.Add(_small);
        foreach (var child in _small.Children)
        {
            var tb = (child as TextBlock)!;
            int n = Grid.GetRow(tb) * 3 + Grid.GetColumn(tb) + 1;
            if (numbers.Contains(n)) tb.Text = n.ToString();
            else tb.Text = "";
        }
    }

    public void UnHighLight()
    {
        _big.Background = new SolidColorBrush(Colors.White);
        foreach (var child in _small.Children)
        {
            ((TextBlock)child)!.Background = new SolidColorBrush(Colors.White);
        }
    }

    public void HighLightBig()
    {
        _big.Background = new SolidColorBrush(Colors.Aqua);
    }

    public void HighLightSmall(int n)
    {
        foreach (var child in _small.Children)
        {
            var tb = (child as TextBlock)!;
            int a = Grid.GetRow(tb) * 3 + Grid.GetColumn(tb) + 1;
            if(a == n) tb.Background = new SolidColorBrush(Colors.Aqua);
        }
    }
}