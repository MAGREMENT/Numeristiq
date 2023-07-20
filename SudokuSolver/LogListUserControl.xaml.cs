using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class LogListUserControl : UserControl
{
    private readonly StackPanel _list;
    
    public LogListUserControl()
    {
        InitializeComponent();

        _list = (FindName("List") as StackPanel)!;
    }

    public void InitLogs(List<ISolverLog> logs)
    {
        _list.Children.Clear();

        foreach (var log in logs)
        {
            var luc = new LogUserControl();
            luc.InitLog(log);
            _list.Children.Add(luc);
        }
    }
}