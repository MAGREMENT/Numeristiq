using System.Collections.Generic;
using System.Windows;
using Model.Solver.Helpers.Logs;
using View.Utils;

namespace View;

public partial class LogListUserControl : ILogListGraphics
{ 
    public event OnShowLogAsked? ShowLogAsked;
    public event OnShowCurrentAsked? ShowCurrentAsked;
    public event OnShowStartAsked? ShowStartAsked;

    private LogUserControl? _currentlyShowed;
    private StateShownType _shownType = StateShownType.After;

    public LogListUserControl()
    {
        InitializeComponent();
    }

    public void InitLogs(List<ISolverLog> logs)
    {
        List.Children.Clear();

        foreach (var log in logs)
        {
            var luc = new LogUserControl();
            luc.InitLog(log);
            luc.SetShownType(_shownType);
            luc.LogClicked += _ => ShowLog(luc);
            luc.ShownTypeChanged += ChangeShownType;
            List.Children.Add(luc);
        }
        
        Scroll.ScrollToBottom();
    }

    public void FocusLog(ISolverLog log)
    {
        var toFocus = (LogUserControl)List.Children[log.Id - 1];
        if (toFocus.Log is null || toFocus.Log.Id != log.Id) return;

        FocusLog(toFocus);
    }
    
    public void UnFocusLog()
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = null;
    }

    private void FocusLog(LogUserControl logUserControl)
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = logUserControl;
        _currentlyShowed.CurrentlyShowed();
    }

    private void ShowLog(LogUserControl logUserControl)
    {
        FocusLog(logUserControl);
        ShowLogAsked?.Invoke(logUserControl.Log!, _shownType);
    }
    
    private void ShowStart(object sender, RoutedEventArgs e)
    {
        UnFocusLog();
        ShowStartAsked?.Invoke();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        UnFocusLog();
        ShowCurrentAsked?.Invoke();
    }

    private void ChangeShownType(StateShownType type)
    {
        if (_shownType == type) return;

        _shownType = type;
        foreach (var child in List.Children)
        {
            if (child is not LogUserControl luc) continue;
            luc.SetShownType(type);
        }
        
        if(_currentlyShowed is not null) ShowLogAsked?.Invoke(_currentlyShowed.Log!, _shownType);
    }
}