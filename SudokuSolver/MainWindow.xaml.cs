using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Logs;

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

            GetLogListUserControl().ShowCurrentClicked += () => GetSudokuUserControl().ShowCurrent();
            GetLogListUserControl().LogClicked += (log) => GetSudokuUserControl().ShowLog(log);
            
            GetLiveModificationUserControl().LiveModified += (number, row, col, action) =>
            {
                if (action == SolverAction.NumberAdded) GetSudokuUserControl().AddDefinitiveNumber(number, row, col);
                else if(action == SolverAction.PossibilityRemoved) GetSudokuUserControl().RemovePossibility(number, row, col);
                GetLogListUserControl().InitLogs(GetSudokuUserControl().GetLogs());
            };
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            string asString = ((TextBox) _main.Children[1]).Text;
            if (_createNewSudoku) GetSudokuUserControl().NewSolver(
                new Solver(new Sudoku(asString)));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e) //TODO fix : update string value when solve
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            SudokuUserControl suc = GetSudokuUserControl();

            bool? stepByStep = ((CheckBox)_aside.Children[0]).IsChecked;
            if (stepByStep is null || (bool) !stepByStep) suc.SolveSudoku();
            else suc.RunUntilProgress();

            GetLogListUserControl().InitLogs(suc.GetLogs());
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            SudokuUserControl suc = GetSudokuUserControl();
            suc.ClearSudoku();
            
            GetLogListUserControl().InitLogs(suc.GetLogs());
        }

        /*Gets*/
        private SudokuUserControl GetSudokuUserControl()
        {
            return (SudokuUserControl)_main.Children[0];
        }

        private LiveModificationUserControl GetLiveModificationUserControl()
        {
            return (LiveModificationUserControl)_aside.Children[2];
        }

        private LogListUserControl GetLogListUserControl()
        {
            return (LogListUserControl)_aside.Children[1];
        }
    }
