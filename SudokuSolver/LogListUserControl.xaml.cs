using System.Collections.Generic;
using System.Windows;
using Model.Solver.Helpers.Logs;

namespace SudokuSolver;

public partial class LogListUserControl
{ 
    public event LogUserControl.OnLogClicked? LogClicked;
    
    public delegate void OnShowCurrentClicked();
    public event OnShowCurrentClicked? ShowCurrentClicked;

    public delegate void OnShowStartClicked();
    public event OnShowStartClicked? ShowStartClicked;

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
        LogClicked?.Invoke(logUserControl.Log!);
    }
    
    private void ShowStart(object sender, RoutedEventArgs e)
    {
        UnFocusLog();
        ShowStartClicked?.Invoke();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        UnFocusLog();
        ShowCurrentClicked?.Invoke();
    }

    public void FocusLog(int number)
    {
        if (number < 0 || number >= List.Children.Count) return;
        
        FocusLog((LogUserControl)List.Children[number]);
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