using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model.Sudoku;
using Presenter.Sudoku.Translators;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class LogViewerUserControl : UserControl
{
    private bool _invokeStateShowEvent;
    
    public delegate void OnLogHighlightShift(int shift);
    public event OnLogHighlightShift? LogHighlightShifted;
    
    public delegate void OnStateShownChange(StateShown ss);
    public event OnStateShownChange? StateShownChanged;

    public delegate void OnExplanationAsked();
    public event OnExplanationAsked? ExplanationAsked;
    
    public LogViewerUserControl()
    {
        InitializeComponent();
        
        StopShowing();
    }

    public void StopShowing()
    {
        Title.Text = " ";
        HighlightsNumber.Text = "0 / 0";
        Explanation.Text = "";
    }

    public void Show(ViewLog log)
    {
        Title.Text = log.Title;
        Title.Foreground = new SolidColorBrush(ColorUtility.ToColor(log.Intensity));
        HighlightsNumber.Text = log.HighlightCursor;
        Explanation.Text = $"{log.Explanation}\n=>{log.Changes}";
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
        LogHighlightShifted?.Invoke(-1);
    }

    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        LogHighlightShifted?.Invoke(1);
    }

    private void ToBefore(object sender, RoutedEventArgs e)
    {
        if (_invokeStateShowEvent) StateShownChanged?.Invoke(StateShown.Before);
    }

    private void ToAfter(object sender, RoutedEventArgs e)
    {
        if (_invokeStateShowEvent) StateShownChanged?.Invoke(StateShown.After);
    }

    private void ShowExplanation(object sender, RoutedEventArgs e)
    {
        ExplanationAsked?.Invoke();
    }
}