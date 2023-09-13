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
            luc.LogClicked += logClicked => ShowLog(luc, logClicked);
            List.Children.Add(luc);
        }
        
        Scroll.ScrollToBottom();
    }

    private void ShowLog(LogUserControl logUserControl, ISolverLog log)
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = logUserControl;
        logUserControl.CurrentlyShowed();
        
        LogClicked?.Invoke(log);
    }
    
    private void ShowStart(object sender, RoutedEventArgs e)
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = null;

        ShowStartClicked?.Invoke();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        _currentlyShowed?.NotShowedAnymore();
        _currentlyShowed = null;

        ShowCurrentClicked?.Invoke();
    }
}