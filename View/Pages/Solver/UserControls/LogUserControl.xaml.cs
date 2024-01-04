using System.Windows;
using System.Windows.Media;
using Global.Enums;
using Presenter.Translators;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class LogUserControl
{
    public delegate void OnStateShownChange(StateShown ss);
    public event OnStateShownChange? StateShownChanged;
    
    public delegate void OnHighlightShift(int shift);
    public event OnHighlightShift? HighlightShifted;

    private bool _invokeStateShowEvent = true;
    
    public LogUserControl()
    {
        InitializeComponent();

        Main.MouseEnter += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.LightGray);
        };
        Main.MouseLeave += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.White);
        };
    }

    public void InitLog(ViewLog log)
    {
        Number.Text = "#" + log.Id;
        Title.Foreground = new SolidColorBrush(ColorUtility.ToColor(log.Intensity));
        Title.Text = log.Title;
        Text.Text = log.Changes;

        HighlightsNumber.Text = log.HighlightCursor;
        if (log.HighlightCount <= 1)
        {
            LeftArrow.IsEnabled = false;
            RightArrow.IsEnabled = false;
        }
    }

    public void CurrentlyFocused()
    {
        Main.BorderBrush = Brushes.Orange;
        Highlights.Visibility = Visibility.Visible;
    }

    public void NotFocusedAnymore()
    {
        Main.BorderBrush = Brushes.White;
        Highlights.Visibility = Visibility.Hidden;
    }

    private void ShowStateBefore(object sender, RoutedEventArgs e)
    {
        if(_invokeStateShowEvent) StateShownChanged?.Invoke(StateShown.Before);
    }

    private void ShowStateAfter(object sender, RoutedEventArgs e)
    {
        if(_invokeStateShowEvent) StateShownChanged?.Invoke(StateShown.After);
    }

    public void SetShownType(StateShown type)
    {
        _invokeStateShowEvent = false;
        
        if (type == StateShown.After) TypeAfter.IsChecked = true;
        else TypeBefore.IsChecked = true;
        
        _invokeStateShowEvent = true;
    }

    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(-1);
    }

    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(1);
    }
}