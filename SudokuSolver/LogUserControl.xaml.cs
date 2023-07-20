using System;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using Model.Logs;

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
        _title.Foreground = new SolidColorBrush(log.Intensity switch
        {
            Intensity.Zero => Colors.Gray,
            Intensity.One => Colors.RoyalBlue,
            Intensity.Two => Colors.Green,
            Intensity.Three => Colors.Orange,
            Intensity.Four => Colors.Red,
            Intensity.Five => Colors.Purple,
            Intensity.Six => Colors.Black,
            _ => Colors.Gray
        });
        _title.Text = log.Title;
        _text.Text = log.Text;
    }
}