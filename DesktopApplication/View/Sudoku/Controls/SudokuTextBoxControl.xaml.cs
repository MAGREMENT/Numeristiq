using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class SudokuTextBoxControl
{
    private bool _callNewSudoku = true;
    
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
        _callNewSudoku = false;
        TextBox.Text = s;
        _callNewSudoku = true;
    }

    private void TextChanged(object sender, TextChangedEventArgs e)
    {
        if(_callNewSudoku) NewSudoku?.Invoke(TextBox.Text);
    }
}

public delegate void OnNewSudoku(string s);
public delegate void OnShow();