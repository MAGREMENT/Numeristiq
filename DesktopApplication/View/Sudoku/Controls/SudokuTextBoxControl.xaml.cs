using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SudokuTextBoxControl
{
    public event OnNewSudoku? NewSudoku;
    public event OnShow? Showed;
    
    public SudokuTextBoxControl()
    {
        InitializeComponent();
    }

    private void Show(object sender, RoutedEventArgs e)
    {
        UpperPart.Visibility = UpperPart.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        if(UpperPart.Visibility == Visibility.Visible) Showed?.Invoke();
    }

    public void SetText(string s)
    {
        TextBox.Text = s;
    }

    private void TextChanged(object sender, TextChangedEventArgs e)
    {
        NewSudoku?.Invoke(TextBox.Text);
    }
}

public delegate void OnNewSudoku(string s);
public delegate void OnShow();