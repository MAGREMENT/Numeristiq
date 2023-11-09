using System.Windows;
using System.Windows.Media;
using Model;
using Model.Solver;
using Model.Solver.Helpers;
using SudokuSolver.Utils;

namespace SudokuSolver;

public partial class StrategyUserControl
{
    private bool _invoke = true;
    
    public delegate void OnStrategyExclusion();
    public event OnStrategyExclusion? Excluded;

    public delegate void OnStrategyUse();
    public event OnStrategyUse? Used;
    
    public StrategyUserControl()
    {
        InitializeComponent();
    }

    public void Update(StrategyInfo strategy)
    {
        _invoke = false;
        
        StrategyName.Text = strategy.StrategyName;
        StrategyName.Foreground = new SolidColorBrush(ColorManager.ToColor(strategy.Locked ? StrategyDifficulty.None : strategy.Difficulty));
        StrategyUsage.IsChecked = strategy.Used;
        StrategyUsage.IsEnabled = !strategy.Locked;

        _invoke = true;
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (!_invoke) return;
        Used?.Invoke();
    }

    private void OnUnChecked(object sender, RoutedEventArgs e)
    {
        if (!_invoke) return;
        Excluded?.Invoke();
    }

    public void Highlight()
    {
        Background = ColorManager.Green;
    }

    public void UnHighlight()
    {
        Background = ColorManager.Background1;
    }
}