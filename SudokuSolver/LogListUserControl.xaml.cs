using System.Collections.Generic;
using System.Windows;
using Model.Logs;

namespace SudokuSolver;

public partial class LogListUserControl
{ 
    public event LogUserControl.OnLogClicked? LogClicked;
    
    public delegate void OnShowCurrentClicked();
    public event OnShowCurrentClicked? ShowCurrentClicked;
    
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
            luc.LogClicked += logClicked =>
            {
                LogClicked?.Invoke(logClicked);
            };
            List.Children.Add(luc);
        }
        
        Scroll.ScrollToBottom();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        ShowCurrentClicked?.Invoke();
    }
}