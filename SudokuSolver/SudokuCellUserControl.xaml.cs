using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model.Logs;
using Model.Possibilities;

namespace SudokuSolver;

public partial class SudokuCellUserControl : UserControl
{
    private readonly NumbersUserControl _numbers;

    private bool _isPossibilities = false;
    private int[] _nums = Array.Empty<int>();

    public delegate void OnClickedOn(SudokuCellUserControl sender);
    public event OnClickedOn? ClickedOn;

    public delegate void OnUpdate(bool isPossibilities, int[] numbers);
    public event OnUpdate? Updated;

    public SudokuCellUserControl()
    {
        InitializeComponent();

        _numbers = (FindName("Numbers") as NumbersUserControl)!;
        _numbers.SetSize(56);
        _numbers.MouseLeftButtonDown += (_, _) =>
        {
            ClickedOn?.Invoke(this);
        };
    }

    public void HighLightPossibility(int possibility, Color color)
    {
        _numbers.HighLightSmall(possibility, color);
    }

    public void HighLight(Color color)
    {
        _numbers.HighLightBig(color);
    }

    public void UnHighLight()
    {
        _numbers.UnHighLight();
    }

    public void SetDefinitiveNumber(int number)
    {
        _numbers.SetBig(number);
        
        _isPossibilities = false;
        _nums = new[] { number };
        Updated?.Invoke(false, _nums);
    }

    public void SetPossibilities(IPossibilities possibilities)
    {
        _isPossibilities = true;
        _nums = possibilities.ToArray();
        
        _numbers.SetSmall(_nums);
        
        Updated?.Invoke(_isPossibilities, _nums);
    }

    public void SetPossibilities(List<int> possibilities)
    {
        _isPossibilities = true;
        _nums = possibilities.ToArray();
        
        _numbers.SetSmall(_nums);
        
        Updated?.Invoke(_isPossibilities, _nums);
    }
    
    public void FireUpdated()
    {
        Updated?.Invoke(_isPossibilities, _nums);
    }

    public bool BorderTop
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("B00") as StackPanel)!.Visibility = vis;
            (FindName("B01") as StackPanel)!.Visibility = vis;
            (FindName("B02") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderBottom
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("B20") as StackPanel)!.Visibility = vis;
            (FindName("B21") as StackPanel)!.Visibility = vis;
            (FindName("B22") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderLeft
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("B00") as StackPanel)!.Visibility = vis;
            (FindName("B10") as StackPanel)!.Visibility = vis;
            (FindName("B20") as StackPanel)!.Visibility = vis;
        }
    }
    
    public bool BorderRight
    {
        set
        {
            var vis = value ? Visibility.Visible : Visibility.Hidden;
            (FindName("B02") as StackPanel)!.Visibility = vis;
            (FindName("B12") as StackPanel)!.Visibility = vis;
            (FindName("B22") as StackPanel)!.Visibility = vis;
        }
    }
}