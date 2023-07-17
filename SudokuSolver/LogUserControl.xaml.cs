using System;
using System.Windows.Controls;
using System.Windows.Media;
using Model;

namespace SudokuSolver;

public partial class LogUserControl : UserControl
{
    private readonly TextBlock _title;
    private readonly TextBlock _text;
    
    public LogUserControl()
    {
        InitializeComponent();

        _title = (FindName("Title") as TextBlock)!;
        _text = (FindName("Text") as TextBlock)!;
    }

    public void InitLog(ISolverLog log)
    {
        _title.Foreground = new SolidColorBrush(log.Level switch
        {
            StrategyLevel.None => Colors.Gray,
            StrategyLevel.Basic => Colors.RoyalBlue,
            StrategyLevel.Easy => Colors.Green,
            StrategyLevel.Medium => Colors.Orange,
            StrategyLevel.Hard => Colors.Red,
            StrategyLevel.ByTrial => Colors.Black,
            _ => Colors.Gray
        });
        _title.Text = log.Level.ToString();
        _text.Text = log.AsString;
    }
}