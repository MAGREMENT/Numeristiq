using System;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using Model.Logs;

namespace SudokuSolver;

public partial class LogUserControl : UserControl
{
    private readonly StackPanel _main;
    private readonly TextBlock _title;
    private readonly TextBlock _text;
    
    public LogUserControl()
    {
        InitializeComponent();

        _main = (FindName("Main") as StackPanel)!;
        _title = (FindName("Title") as TextBlock)!;
        _text = (FindName("Text") as TextBlock)!;

        _main.MouseEnter += (_, _) =>
        {
            _main.Background = new SolidColorBrush(Colors.WhiteSmoke);
        };
        _main.MouseLeave += (_, _) =>
        {
            _main.Background = new SolidColorBrush(Colors.White);
        };
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