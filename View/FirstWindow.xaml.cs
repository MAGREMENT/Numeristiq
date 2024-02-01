using System.Windows;

namespace View;

public partial class FirstWindow
{
    private MainSudokuWindow? _sudoku;
    private MainTectonicWindow? _tectonic;
    
    public FirstWindow()
    {
        InitializeComponent();
    }

    private void GoToTectonic(object sender, RoutedEventArgs e)
    {
        _tectonic ??= new MainTectonicWindow();
        _tectonic.Show();
        Close();
    }

    private void GoToSudoku(object sender, RoutedEventArgs e)
    {
        _sudoku ??= new MainSudokuWindow();
        _sudoku.Show();
        Close();
    }
}