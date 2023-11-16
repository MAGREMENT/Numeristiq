using System.Windows;
using System.Windows.Media;
using Global.Enums;
using Presenter.Translator;
using View.Utils;

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
        StrategyName.Foreground = new SolidColorBrush(ColorManager.ToColor(strategy.Locked ? Intensity.Zero : strategy.Intensity));
        StrategyUsage.IsChecked = strategy.Used;
        StrategyUsage.IsEnabled = !strategy.Locked;

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
        Background = ColorManager.Green;
    }

    public void UnLightUp()
    {
        Background = ColorManager.Background1;
    }
}