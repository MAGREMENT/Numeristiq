using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Model.Sudoku.Solver;

namespace DesktopApplication.View.Sudoku.Controls;

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
    }

    public void SetHighlight(bool highlighted)
    {
        TextBlock.SetResourceReference(ForegroundProperty, highlighted ? "Primary1" : "Text");
    }

    public void EnableStrategy(bool enabled)
    {
        if (enabled)
        {
            CheckMark.Stroke = Brushes.ForestGreen;
            CheckMark.Visibility = Visibility.Visible;
            CrossMark.Visibility = Visibility.Collapsed;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Left;

            _state = 1;
        }
        else
        {
            CrossMark.Stroke = Brushes.Red;
            CrossMark.Visibility = Visibility.Visible;
            CheckMark.Visibility = Visibility.Collapsed;
            TextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            _state = 0;
        }
    }

    public void LockStrategy()
    {
        CheckMark.Stroke = Brushes.Gray;
        CrossMark.Stroke = Brushes.Gray;

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