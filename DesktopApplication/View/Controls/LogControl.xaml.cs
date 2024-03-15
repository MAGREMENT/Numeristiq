using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.View.Utility;
using Model;
using Model.Helpers.Highlighting;
using Model.Helpers.Logs;

namespace DesktopApplication.View.Controls;

public partial class LogControl
{
    private readonly int _id;
    private bool _shouldCallStateShownEvent = true;

    public event OnOpenRequest? OpenRequested;
    public event OnStateShownChange? StateShownChanged;
    public event OnHighlightShift? HighlightShifted;
    
    public LogControl(ISolverLog log, StateShown stateShown)
    {
        InitializeComponent();

        _id = log.Id;

        Title.Text = log.Title;
        Title.Foreground = new SolidColorBrush(ColorUtility.ToColor(log.Intensity));
        Number.Text = $"#{log.Id}";
        HighlightCount.Text = log.GetCursorPosition();
        SetStateShown(stateShown);
        TextOutput.Text = log.Description;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.BorderBrush = (Brush)Application.Current.Resources["Primary1"]!;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.BorderBrush = (Brush)Application.Current.Resources["Background2"]!;
    }

    public void Open()
    {
        BottomPart.Visibility = Visibility.Visible;
    }

    public void Close()
    {
        BottomPart.Visibility = Visibility.Collapsed;
    }

    public void SetStateShown(StateShown stateShown)
    {
        _shouldCallStateShownEvent = false;
        if (stateShown == StateShown.Before) BeforeButton.IsChecked = true;
        else AfterButton.IsChecked = true;
        _shouldCallStateShownEvent = true;
    }


    public void SetCursorPosition(string s)
    {
        HighlightCount.Text = s;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
        OpenRequested?.Invoke(_id);
    }

    private void BeforeChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.Before);
    }
    
    private void AfterChecked(object sender, RoutedEventArgs e)
    {
        if (_shouldCallStateShownEvent) StateShownChanged?.Invoke(StateShown.After);
    }
    
    private void ShiftLeft(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(true);
    }
    
    private void ShiftRight(object sender, RoutedEventArgs e)
    {
        HighlightShifted?.Invoke(false);
    }
}

public delegate void OnOpenRequest(int id);
public delegate void OnStateShownChange(StateShown stateShown);
public delegate void OnHighlightShift(bool isLeft);