using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StackPanel _main;
        private readonly StackPanel _aside;

        private bool _justSolvedSudoku = false;
        
        public MainWindow()
        {
            InitializeComponent();

            _main = (FindName("Main") as StackPanel)!;
            _aside = (FindName("Aside") as StackPanel)!;

            ((SudokuUserControl)_main.Children[0]).IsReady += () =>
            {
                ((Button)((StackPanel)_main.Children[2]).Children[0]).IsEnabled = true;
            };
        }

        private void UpdateSudoku(string asString)
        {
            if (!_justSolvedSudoku) ((SudokuUserControl)_main.Children[0]).InitSolver(new Solver(new Sudoku(asString)));
            else _justSolvedSudoku = false;
        }

        private void UpdateSudoku(object sender, TextChangedEventArgs e)
        {
            UpdateSudoku(((TextBox) _main.Children[1]).Text);
        }

        private void SolveSudoku(object sender, RoutedEventArgs e)
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            bool? stepByStep = ((CheckBox)_aside.Children[1]).IsChecked;
            if (stepByStep is null || (bool) !stepByStep) ((SudokuUserControl) _main.Children[0]).SolveSudoku();
            else ((SudokuUserControl) _main.Children[0]).RunUntilProgress();

            _justSolvedSudoku = true;

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