using System.Windows;
using Presenter.Translator;

namespace View.Pages.StrategyManager.UserControls;

public partial class OptionModifierUserControl
{
    public OptionModifierUserControl()
    {
        InitializeComponent();
    }

    public void Show(ViewStrategy strategy)
    {
        Panel.Visibility = Visibility.Visible;
        StrategyName.Text = strategy.Name;
        StrategyUsage.IsChecked = strategy.Used;
    }
}