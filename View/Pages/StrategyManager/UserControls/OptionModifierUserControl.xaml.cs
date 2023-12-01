using System.Windows;
using System.Windows.Controls;
using Model.Solver;
using Presenter.Translators;

namespace View.Pages.StrategyManager.UserControls;

public partial class OptionModifierUserControl
{
    private bool _callEvents = true;

    public event OnBehaviorChange? BehaviorChanged;
    public event OnUsageChange? UsageChanged;
    
    public OptionModifierUserControl()
    {
        InitializeComponent();
    }

    public void Show(ViewStrategy strategy)
    {
        _callEvents = false;
        Panel.Visibility = Visibility.Visible;
        StrategyName.Text = strategy.Name;
        StrategyUsage.IsChecked = strategy.Used;
        StrategyBehavior.SelectedIndex = (int)strategy.Behavior;
        _callEvents = true;
    }

    public void Hide()
    {
        Panel.Visibility = Visibility.Hidden;
    }

    private void ChangeBehavior(object sender, SelectionChangedEventArgs e)
    {
        if(_callEvents) BehaviorChanged?.Invoke(StrategyName.Text, (OnCommitBehavior)StrategyBehavior.SelectedIndex);
    }

    private void ChangeUsageToYes(object sender, RoutedEventArgs e)
    {
        if(_callEvents) UsageChanged?.Invoke(StrategyName.Text, true);
    }
    
    private void ChangeUsageToNo(object sender, RoutedEventArgs e)
    {
        if(_callEvents) UsageChanged?.Invoke(StrategyName.Text, false);
    }
}

public delegate void OnUsageChange(string name, bool yes);
public delegate void OnBehaviorChange(string name, OnCommitBehavior behavior);