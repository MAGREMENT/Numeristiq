using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly StackPanel _main;
        private readonly StackPanel _aside;

        private bool _createNewSudoku = true;
        
        public MainWindow()
        {
            InitializeComponent();

            _main = (FindName("Main") as StackPanel)!;
            _aside = (FindName("Aside") as StackPanel)!;

            GetSudokuUserControl().IsReady += () =>
            {
                ((Button)((StackPanel)_main.Children[2]).Children[0]).IsEnabled = true;
            };

            GetSudokuUserControl().CellClickedOn += (sender, row, col) =>
            {
                GetLiveModificationUserControl().SetCurrent(sender, row, col);
            };

            GetSudokuUserControl().SolverUpdated += (asString) =>
            {
                _createNewSudoku = false;
                ((TextBox)_main.Children[1]).Text = asString;
                _createNewSudoku = true;
            };
            
            GetLiveModificationUserControl().Init(GetSudokuUserControl());
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            string asString = ((TextBox) _main.Children[1]).Text;
            if (_createNewSudoku) GetSudokuUserControl().NewSolver(
                new Solver(new Sudoku(asString)));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e)
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            SudokuUserControl suc = GetSudokuUserControl();

            bool? stepByStep = ((CheckBox)_aside.Children[1]).IsChecked;
            if (stepByStep is null || (bool) !stepByStep) suc.SolveSudoku();
            else suc.RunUntilProgress();

            (FindName("Logs") as LogListUserControl)!.InitLogs(suc.GetLogs());
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            SudokuUserControl suc = GetSudokuUserControl();
            suc.ClearSudoku();
            
            (FindName("Logs") as LogListUserControl)!.InitLogs(suc.GetLogs());
        }

        private void SeePossibilities(object sender, RoutedEventArgs e)
        {
            GetSudokuUserControl().SeePossibilities = true;
        }

        private void UnSeePossibilities(object sender, RoutedEventArgs e)
        {
            GetSudokuUserControl().SeePossibilities = false;
        }
        
        
        /*Gets*/
        private SudokuUserControl GetSudokuUserControl()
        {
            return (SudokuUserControl)_main.Children[0];
        }

        private LiveModificationUserControl GetLiveModificationUserControl()
        {
            return (LiveModificationUserControl)_aside.Children[3];
        }
    }
