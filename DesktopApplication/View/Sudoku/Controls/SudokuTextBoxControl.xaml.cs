using System.Windows;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SudokuTextBoxControl
{
    public SudokuTextBoxControl()
    {
        InitializeComponent();
    }

    private void Show(object sender, RoutedEventArgs e)
    {
        UpperPart.Visibility = UpperPart.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    public void SetText(string s)
    {
        TextBox.Text = s;
    }
}