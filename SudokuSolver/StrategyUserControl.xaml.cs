using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model;

namespace SudokuSolver;

public partial class StrategyUserControl
{
    private readonly TextBlock _name;

    public delegate void OnStrategyExclusion();
    public event OnStrategyExclusion? Excluded;

    public delegate void OnStrategyUse();
    public event OnStrategyUse? Used;
    
    public StrategyUserControl()
    {
        InitializeComponent();

        _name = (FindName("Name") as TextBlock)!;
    }

    public void InitStrategy(IStrategy strategy)
    {
        _name.Text = strategy.Name;
        _name.Foreground = new SolidColorBrush(ColorUtil.ToColor(strategy.Difficulty));
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        Used?.Invoke();
    }

    private void OnUnChecked(object sender, RoutedEventArgs e)
    {
        Excluded?.Invoke();
    }
}