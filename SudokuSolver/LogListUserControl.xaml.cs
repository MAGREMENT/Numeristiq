using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class LogListUserControl : UserControl
{
    private readonly StackPanel _list;
    private readonly ScrollViewer _scroll;

    public event LogUserControl.OnLogClicked? LogClicked;
    
    public delegate void OnShowCurrentClicked();
    public event OnShowCurrentClicked? ShowCurrentClicked;
    
    public LogListUserControl()
    {
        InitializeComponent();

        _list = (FindName("List") as StackPanel)!;
        _scroll = (FindName("Scroll") as ScrollViewer)!;
    }

    public void InitLogs(List<ISolverLog> logs)
    {
        _list.Children.Clear();

        foreach (var log in logs)
        {
            var luc = new LogUserControl();
            luc.InitLog(log);
            luc.LogClicked += logClicked =>
            {
                LogClicked?.Invoke(logClicked);
            };
            _list.Children.Add(luc);
        }
        
        _scroll.ScrollToBottom();
    }

    private void ShowCurrent(object sender, RoutedEventArgs e)
    {
        ShowCurrentClicked?.Invoke();
    }
}