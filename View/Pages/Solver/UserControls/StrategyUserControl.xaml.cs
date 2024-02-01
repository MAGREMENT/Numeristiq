using System.Windows;
using System.Windows.Media;
using Model.Sudoku.Solver.Helpers.Logs;
using Presenter.Sudoku.Translators;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class StrategyUserControl
{
    private bool _invoke = true;

    public delegate void OnUsageChange(bool used);
    public event OnUsageChange? UsageChanged;
    
    public StrategyUserControl()
    {
        InitializeComponent();
    }

    public void Match(ViewStrategy strategy)
    {
        _invoke = false;
        
        StrategyName.Text = strategy.Name;
        StrategyName.Foreground = new SolidColorBrush(ColorUtility.ToColor(strategy.Locked ? Intensity.Zero : strategy.Intensity));
        StrategyUsage.IsChecked = strategy.Used;
        StrategyUsage.IsEnabled = !strategy.Locked;

        _invoke = true;
    }

    public void Check(bool yes)
    {
        if (!StrategyUsage.IsEnabled) return;
        
        _invoke = false;
        StrategyUsage.IsChecked = yes;
        _invoke = true;
    }

    private void OnChecked(object sender, RoutedEventArgs e)
    {
        if (!_invoke) return;
        UsageChanged?.Invoke(true);
    }

    private void OnUnChecked(object sender, RoutedEventArgs e)
    {
        if (!_invoke) return;
        UsageChanged?.Invoke(false);
    }

    public void LightUp()
    {
        Background = Brushes.Green;
    }

    public void UnLightUp()
    {
        Background = Brushes.White;
    }
}