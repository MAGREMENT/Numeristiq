using System.Windows;
using System.Windows.Media;
using Model;

namespace SudokuSolver;

public partial class StrategyUserControl
{
    public delegate void OnStrategyExclusion();
    public event OnStrategyExclusion? Excluded;

    public delegate void OnStrategyUse();
    public event OnStrategyUse? Used;
    
    public StrategyUserControl()
    {
        InitializeComponent();
    }

    public void InitStrategy(IStrategy strategy)
    {
        StrategyName.Text = strategy.Name;
        StrategyName.Foreground = new SolidColorBrush(ColorUtil.ToColor(strategy.Difficulty));
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