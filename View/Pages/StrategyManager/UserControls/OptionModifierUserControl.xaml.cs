using System.Windows;
using System.Windows.Controls;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.Arguments;
using Presenter.Sudoku.Translators;
using View.Canvas;

namespace View.Pages.StrategyManager.UserControls;

public partial class OptionModifierUserControl
{
    private const int OptionsFontSize = 15;
    
    private bool _callEvents = true;

    public event OnBehaviorChange? BehaviorChanged;
    public event OnUsageChange? UsageChanged;
    public event OnArgumentChange? ArgumentChanged;
    
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

        for (int i = Panel.Children.Count - 1; i >= 1; i--)
        {
            Panel.Children.RemoveAt(i);
        }

        if (strategy.Arguments.Count == 0) return;

        StackPanel sp = new()
        {
            Orientation = Orientation.Vertical
        };
        foreach (var i in strategy.Arguments)
        {
            OptionCanvas? optionCanvas = i.Interface switch
            {
                SliderViewInterface svi => new SliderOptionCanvas(i.Name, "", svi.Min,
                    svi.Max, svi.TickFrequency, () => i.CurrentValue.ToInt(), n =>
                    {
                        ArgumentChanged?.Invoke(strategy.Name, i.Name, new IntArgumentValue(n));
                    }),
                BooleanViewInterface => new CheckBoxOptionCanvas(i.Name, "", 
                    () => i.CurrentValue.ToBool(), n =>
                    {
                        ArgumentChanged?.Invoke(strategy.Name, i.Name, new BoolArgumentValue(n));
                    }),
                MinMaxSliderViewInterface mm => new MinMaxSliderOptionCanvas(i.Name, "",
                    mm.MinMin, mm.MinMax, mm.MaxMin, mm.MaxMax, mm.TickFrequency, () => i.CurrentValue.ToMinMax(),
                    n =>
                    {
                        ArgumentChanged?.Invoke(strategy.Name, i.Name, new MinMaxArgumentValue(n));
                    }),
                _ => null
            };

            if (optionCanvas is null) continue;

            optionCanvas.SetFontSize(OptionsFontSize);
            optionCanvas.Margin = new Thickness(0, 5, 0, 0);
            optionCanvas.Refresh();
            sp.Children.Add(optionCanvas);
        }

        if (sp.Children.Count == 0) return;
        
        Panel.Children.Add(new Separator
        {
            Margin = new Thickness(0, 10, 0, 10)
        });
        Panel.Children.Add(sp);
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
public delegate void OnArgumentChange(string strategyName, string argumentName, ArgumentValue value);