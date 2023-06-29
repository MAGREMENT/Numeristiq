using System.Windows;
using System.Windows.Controls;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StackPanel _main;
        private readonly StackPanel _aside;
        
        public MainWindow()
        {
            InitializeComponent();

            _main = (FindName("Main") as StackPanel)!;
            _aside = (FindName("Aside") as StackPanel)!;
        }

        private void UpdateSudoku(string asString)
        {
            ((SudokuUserControl) _main.Children[0]).UpdateIfDifferent(asString);
        }

        private void UpdateSudoku(object sender, TextChangedEventArgs e)
        {
            UpdateSudoku(((TextBox) _main.Children[1]).Text);
        }

        private void SolveSudoku(object sender, RoutedEventArgs e)
        {
            ((SudokuUserControl) _main.Children[0]).SolveSudoku();
            ((TextBox)_main.Children[1]).Text = ((SudokuUserControl)_main.Children[0]).SudokuAsString();
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            ((SudokuUserControl) _main.Children[0]).ClearSudoku();
            ((TextBox) _main.Children[1]).Text = "";
        }

        private void SeePossibilities(object sender, RoutedEventArgs e)
        {
            ((SudokuUserControl) _main.Children[0]).SeePossibilities = true;
        }

        private void UnSeePossibilities(object sender, RoutedEventArgs e)
        {
            ((SudokuUserControl) _main.Children[0]).SeePossibilities = false;
        }
    }
}