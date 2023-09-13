using System.Windows;
using System.Windows.Media;
using Model;
using Model.Solver.Helpers;
using SudokuSolver.Utils;

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

    public void InitStrategy(StrategyInfo strategy)
    {
        StrategyName.Text = strategy.StrategyName;
        StrategyName.Foreground = new SolidColorBrush(ColorUtil.ToColor(strategy.Difficulty));
        StrategyUsage.IsChecked = strategy.Used;
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