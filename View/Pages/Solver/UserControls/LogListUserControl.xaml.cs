using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Global.Enums;
using Presenter.Translators;
using View.Themes;

namespace View.Pages.Solver.UserControls;

public partial class LogListUserControl
{
    private LogUserControl? _currentlyShowed;
    private StateShown _shownType = StateShown.Before;

    public delegate void OnLogSelection(int number);
    public event OnLogSelection? LogSelected;

    public delegate void OnLogShift(int delta);
    public event OnLogShift? LogShifted;
    
    public delegate void OnShowStartStateAsked();
    public event OnShowStartStateAsked? ShowStartStateAsked;

    public delegate void OnShowCurrentStateAsked();
    public event OnShowCurrentStateAsked? ShowCurrentStateAsked;
    
    public event LogUserControl.OnStateShownChange? StateShownChanged;

    public delegate void OnLogHighlightShift(int number, int shift);
    public event OnLogHighlightShift? LogHighlightShifted;

    private Brush _buttonNormalBackground = Brushes.White;
    private Brush _buttonHoverBackground = Brushes.White;

    public LogListUserControl()
    {
        InitializeComponent();
        
        foreach (System.Windows.Controls.Canvas canvas in ButtonPanel.Children)
        {
            canvas.MouseEnter += (_, _) => canvas.Background = _buttonHoverBackground;
            canvas.MouseLeave += (_, _) => canvas.Background = _buttonNormalBackground;
        }
        
        ((App)Application.Current).ThemeChanged += ApplyTheme;
    }

    public void SetLogs(IReadOnlyList<ViewLog> logs)
    {
        List.Children.Clear();

        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            
            var luc = new LogUserControl();
            
            luc.InitLog(log);
            luc.SetShownType(_shownType);

            var iForEvent = i;
            luc.MouseLeftButtonDown += (_, _) => LogSelected?.Invoke(iForEvent);
            luc.StateShownChanged += ss =>
            {
                ChangeStateShown(ss);
                StateShownChanged?.Invoke(ss);
            };
            luc.HighlightShifted += shift => LogHighlightShifted?.Invoke(iForEvent, shift);
            
            List.Children.Add(luc);
        }

        Scroll.ScrollToBottom();
    }
    
    public void UpdateFocusedLog(ViewLog log)
    {
        if (_currentlyShowed == null) return;
        
        _currentlyShowed.InitLog(log);
        _currentlyShowed.SetShownType(_shownType);
    }

    public void FocusLog(int n)
    {
        var children = List.Children;
        if (n < 0 || n >= children.Count) return;
        
        _currentlyShowed?.NotFocusedAnymore();
        _currentlyShowed = (LogUserControl)children[n];
        _currentlyShowed.CurrentlyFocused();
    }
    
    public void UnFocusLog()
    {
        _currentlyShowed?.NotFocusedAnymore();
        _currentlyShowed = null;
    }

    public void ChangeStateShown(StateShown ss)
    {
        if (_shownType == ss) return;

        _shownType = ss;
        foreach (var child in List.Children)
        {
            if (child is not LogUserControl luc) continue;
            luc.SetShownType(ss);
        }
    }

    private void ShowStart(object sender, RoutedEventArgs e)
    {
        ShowStartStateAsked?.Invoke();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        ShowCurrentStateAsked?.Invoke();
    }

    private void ShiftDown(object sender, RoutedEventArgs args)
    {
        LogShifted?.Invoke(1);
    }

    private void ShiftUp(object sender, RoutedEventArgs args)
    {
        LogShifted?.Invoke(-1);
    }

    public void ApplyTheme(Theme theme)
    {
        _buttonNormalBackground = theme.Background2;
        _buttonHoverBackground = theme.Background3;
        foreach (System.Windows.Controls.Canvas canvas in ButtonPanel.Children)
        {
            canvas.Background = _buttonNormalBackground;
        }
        
    }
}