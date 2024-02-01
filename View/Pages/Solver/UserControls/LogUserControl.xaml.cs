using System.Windows;
using System.Windows.Media;
using Presenter.Sudoku.Translators;
using View.Utility;

namespace View.Pages.Solver.UserControls;

public partial class LogUserControl
{ 
    public LogUserControl()
    {
        InitializeComponent();

        Main.MouseEnter += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.LightGray);
        };
        Main.MouseLeave += (_, _) =>
        {
            Main.Background = new SolidColorBrush(Colors.White);
        };
    }

    public void InitLog(ViewLog log)
    {
        Number.Text = "#" + log.Id;
        Title.Foreground = new SolidColorBrush(ColorUtility.ToColor(log.Intensity));
        Title.Text = log.Title;
    }

    public void CurrentlyFocused()
    {
        Main.BorderBrush = Brushes.Orange;
    }

    public void NotFocusedAnymore()
    {
        Main.BorderBrush = Brushes.White;
    }
}