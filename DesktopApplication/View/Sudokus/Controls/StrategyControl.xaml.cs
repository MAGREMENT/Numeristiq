using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using Model.Sudokus.Solver;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class StrategyControl
{
    private int _state = -2;

    public event OnStrategyEnabled? StrategyEnabled;
    
    public StrategyControl(SudokuStrategy strategy)
    {
        InitializeComponent();

        TextBlock.Text = strategy.Name;
        if (strategy.Locked) LockStrategy();
        else EnableStrategy(strategy.Enabled);

        MouseEnter += (_, _) => SetResourceReference(BackgroundProperty, "Background3");
        MouseLeave += (_, _) => SetResourceReference(BackgroundProperty, "Background2");
    }

    public void SetHighlight(bool highlighted)
    {
        TextBlock.SetResourceReference(ForegroundProperty, highlighted ? "Primary1" : "Text");
    }

    public void EnableStrategy(bool enabled)
    {
        if (enabled)
        {
            CheckMark.SetResourceReference(Shape.StrokeProperty, "On");
            CheckMark.Visibility = Visibility.Visible;
            CrossMark.Visibility = Visibility.Collapsed;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Left;

            _state = 1;
        }
        else
        {
            CheckMark.SetResourceReference(Shape.StrokeProperty, "Off");
            CrossMark.Visibility = Visibility.Visible;
            CheckMark.Visibility = Visibility.Collapsed;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            _state = 0;
        }
    }

    public void LockStrategy()
    {
        CheckMark.SetResourceReference(Shape.StrokeProperty, "Disabled");
        CrossMark.SetResourceReference(Shape.StrokeProperty, "Disabled");

        _state = -1;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        if (_state < 0) return;

        bool nextEnabled = _state == 0;
        EnableStrategy(nextEnabled);
        StrategyEnabled?.Invoke(nextEnabled);
    }
}

public delegate void OnStrategyEnabled(bool enabled);