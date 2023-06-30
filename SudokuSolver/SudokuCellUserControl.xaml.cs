using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SudokuSolver;

public partial class SudokuCellUserControl : UserControl
{
    private const int Size = 55;

    private readonly TextBlock _tb;
    private readonly Grid _backGround;

    public SudokuCellUserControl()
    {
        InitializeComponent();

        _tb = (FindName("TB") as TextBlock)!;
        _backGround = (FindName("Background") as Grid)!;

        _backGround.MouseLeftButtonDown += OnClick;
    }

    public void HighLight()
    {
        _backGround.Background = new SolidColorBrush(Colors.Aqua);
    }

    public void UnHighLight()
    {
        _backGround.Background = new SolidColorBrush(Colors.White);
    }

    private void OnClick(object sender, MouseEventArgs e)
    {
        //TODO
    }

    public void SetDefinitiveNumber(int number)
    {
        _tb.FontSize = Size - 5;
        _tb.Foreground = new SolidColorBrush(Colors.Black);
        _tb.Text = number.ToString();
    }

    public void SetPossibilities(List<int> possibilities)
    {
        if (possibilities.Count == 0)
        {
            Void();
            return;
        }
        
        string result = "";
        int counter = 0;
        foreach (var number in possibilities)
        {
            counter++;
            result += number + " ";
            if (counter % 3 == 0) result += "\n";
        }

        result = result[^1] == '\n' ? result[..^1] : result;
        _tb.FontSize = ((double) Size - 5) / 4;
        _tb.Foreground = new SolidColorBrush(Colors.Red);
        _tb.Text = result;
    }

    public void Void()
    {
        _tb.Text = "";
    }

    public bool BorderTop
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("b00") as StackPanel)!.Visibility = vis;
            (FindName("b01") as StackPanel)!.Visibility = vis;
            (FindName("b02") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderBottom
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("b20") as StackPanel)!.Visibility = vis;
            (FindName("b21") as StackPanel)!.Visibility = vis;
            (FindName("b22") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderLeft
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("b00") as StackPanel)!.Visibility = vis;
            (FindName("b10") as StackPanel)!.Visibility = vis;
            (FindName("b20") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderRight
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("b02") as StackPanel)!.Visibility = vis;
            (FindName("b12") as StackPanel)!.Visibility = vis;
            (FindName("b22") as StackPanel)!.Visibility = vis;
        }
    }
}