using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Model;

namespace SudokuSolver;

public partial class SudokuCellUserControl : UserControl
{
    private const int Size = 55;

    private readonly TextBlock _tb;
    private readonly Grid _backGround;

    public delegate void OnClickedOn(SudokuCellUserControl sender);
    public event OnClickedOn? ClickedOn;

    public delegate void OnUpdate();
    public event OnUpdate? Updated;

    public SudokuCellUserControl()
    {
        InitializeComponent();

        _tb = (FindName("TB") as TextBlock)!;
        _backGround = (FindName("Background") as Grid)!;

        _backGround.MouseLeftButtonDown += (_, _) =>
        {
            ClickedOn?.Invoke(this);
        };
    }

    public void HighLight()
    {
        _backGround.Background = new SolidColorBrush(Colors.Aqua);
    }

    public void UnHighLight()
    {
        _backGround.Background = new SolidColorBrush(Colors.White);
    }

    public void SetDefinitiveNumber(int number)
    {
        _tb.FontSize = Size - 5;
        _tb.Foreground = new SolidColorBrush(Colors.Black);
        _tb.Text = number.ToString();
        IsPossibilities = false;
        
        Updated?.Invoke();
    }

    public void SetPossibilities(IPossibilities possibilities)
    {
        if (possibilities.Count == 0)
        {
            Void();
            return;
        }
        
        string result = "";
        int counter = 0;
        foreach (var number in possibilities.All())
        {
            counter++;
            result += number + " ";
            if (counter % 3 == 0) result += "\n";
        }

        result = result[^1] == '\n' ? result[..^1] : result;
        _tb.FontSize = ((double) Size - 5) / 4;
        _tb.Foreground = new SolidColorBrush(Colors.Red);
        _tb.Text = result;
        IsPossibilities = true;
        
        Updated?.Invoke();
    }

    public string Text => _tb.Text;
    
    public bool IsPossibilities { get; private set; }

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