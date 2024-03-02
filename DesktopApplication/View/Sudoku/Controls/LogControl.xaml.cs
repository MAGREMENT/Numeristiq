using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesktopApplication.View.Utility;
using Model.Helpers.Logs;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class LogControl
{
    public LogControl(ISolverLog log)
    {
        InitializeComponent();

        Title.Text = log.Title;
        Title.Foreground = new SolidColorBrush(ColorUtility.ToColor(log.Intensity));
        Number.Text = $"#{log.Id}";
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.BorderBrush = (Brush)Application.Current.Resources["Primary1"]!;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border b) return;

        b.BorderBrush = (Brush)Application.Current.Resources["Background2"]!;
    }
}