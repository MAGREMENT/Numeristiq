using System.Collections.Generic;
using System.Windows;
using Global.Enums;
using Presenter.Translator;

namespace View.Pages.Solver.UserControls;

public partial class LogListUserControl
{
    private LogUserControl? _currentlyShowed;
    private StateShown _shownType = StateShown.After;

    public delegate void OnLogSelection(int number);
    public event OnLogSelection? LogSelected;

    public delegate void OnShowStartStateAsked();
    public event OnShowStartStateAsked? ShowStartStateAsked;

    public delegate void OnShowCurrentStateAsked();
    public event OnShowCurrentStateAsked? ShowCurrentStateAsked;

    public LogListUserControl()
    {
        InitializeComponent();
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
            luc.ShownTypeChanged += ChangeShownType;
            
            List.Children.Add(luc);
        }

        Scroll.ScrollToBottom();
    }

    public void FocusLog(int n)
    {
        var children = List.Children;
        if (n < 0 || n >= children.Count) return;
        
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = (LogUserControl)children[n];
        _currentlyShowed.CurrentlyShowed();
    }
    
    public void UnFocusLog()
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = null;
    }
    
    private void ChangeShownType(StateShown ss)
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
}