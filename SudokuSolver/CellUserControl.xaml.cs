using System.Windows;
using System.Windows.Controls;
using Model.Possibilities;

namespace SudokuSolver;

public partial class CellUserControl
{
    private readonly Grid _small = new();
    private readonly TextBlock _big = new();

    private bool _isPossibilities;
    private IPossibilities _nums = IPossibilities.NewEmpty();

    public delegate void OnClickedOn(CellUserControl sender);
    public event OnClickedOn? ClickedOn;

    public delegate void OnUpdate(bool isPossibilities, IPossibilities numbers);
    public event OnUpdate? Updated;

    public CellUserControl()
    {
        InitializeComponent();

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

        SetSize(57);
        Case.MouseLeftButtonDown += (_, _) =>
        {
            ClickedOn?.Invoke(this);
        };
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
            var tb = (child as TextBlock)!;
            tb.Height = smallSize;
            tb.Width = smallSize;
            tb.FontSize = smallSize * 2 / 3;
        }
    }

    public void SetMargin(int left, int top, int right, int bottom)
    {
        Case.Margin = new Thickness(left, top, right, bottom);
    }

    public void SetDefinitiveNumber(int number)
    {
        Case.Children.Clear();
        Case.Children.Add(_big);
        _big.Text = number.ToString();
        
        _isPossibilities = false;
        _nums = IPossibilities.NewEmpty();
        _nums.Add(number);
        Updated?.Invoke(false, _nums);
    }

    public void SetPossibilities(IPossibilities possibilities)
    {
        _isPossibilities = true;
        _nums = possibilities;
        
        Case.Children.Clear();
        Case.Children.Add(_small);
        foreach (var child in _small.Children)
        {
            var tb = (child as TextBlock)!;
            int n = Grid.GetRow(tb) * 3 + Grid.GetColumn(tb) + 1;
            if (possibilities.Peek(n)) tb.Text = n.ToString();
            else tb.Text = "";
        }
        
        Updated?.Invoke(_isPossibilities, _nums);
    }
    
    public void Void()
    {
        Case.Children.Clear();
    }

    public void FireUpdated()
    {
        Updated?.Invoke(_isPossibilities, _nums);
    }
}