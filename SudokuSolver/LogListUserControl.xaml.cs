using System.Collections.Generic;
using System.Windows;
using Model.Solver.Helpers.Logs;
using SudokuSolver.Utils;

namespace SudokuSolver;

public partial class LogListUserControl : ILogListGraphics
{ 
    public event OnShowLogAsked? ShowLogAsked;
    public event OnShowCurrentAsked? ShowCurrentAsked;
    public event OnShowStartAsked? ShowStartAsked;

    private LogUserControl? _currentlyShowed;

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
            luc.LogClicked += _ => ShowLog(luc);
            List.Children.Add(luc);
        }
        
        Scroll.ScrollToBottom();
    }

    private void ShowLog(LogUserControl logUserControl)
    {
        FocusLog(logUserControl);
        ShowLogAsked?.Invoke(logUserControl.Log!);
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

    public void FocusLog(ISolverLog log)
    {
        var toFocus = (LogUserControl)List.Children[log.Id - 1];
        if (toFocus.Log is null || toFocus.Log.Id != log.Id) return;

        FocusLog(toFocus);
    }

    private void FocusLog(LogUserControl logUserControl)
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = logUserControl;
        _currentlyShowed.CurrentlyShowed();
    }

    public void UnFocusLog()
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = null;
    }
}