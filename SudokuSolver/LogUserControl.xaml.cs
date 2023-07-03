using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver;

public partial class LogUserControl : UserControl
{
    private readonly StackPanel _list;
    
    public LogUserControl()
    {
        InitializeComponent();

        _list = (FindName("List") as StackPanel)!;
    }

    public void InitLogs(List<ISolverLog> logs)
    {
        _list.Children.Clear();

        foreach (var log in logs)
        {
            var tb = new TextBox
            {
                Width = 200,
                Text = log.ViewLog(),
                TextWrapping = TextWrapping.Wrap
            };
            _list.Children.Add(tb);
        }
    }
}