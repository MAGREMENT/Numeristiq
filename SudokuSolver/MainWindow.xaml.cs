using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Solver.Helpers.Changes;

namespace SudokuSolver;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _createNewSudoku = true;

        public MainWindow()
        {
            InitializeComponent();

            Solver.IsReady += () =>
            {
                SolveButton.IsEnabled = true;
                ClearButton.IsEnabled = true;
            };
            Solver.CellClickedOn += (sender, row, col) =>
            {
                LiveModification.SetCurrent(sender, row, col);
            };
            Solver.SolverUpdated += asString =>
            {
                _createNewSudoku = false;
                SudokuStringBox.Text = asString;
                _createNewSudoku = true;
            };
            Solver.LogsUpdated += logs =>
            {
                LogList.InitLogs(logs);
                ExplanationBox.Text = "";
            };
            Solver.LogFocused += LogList.FocusLog;
            Solver.LogUnFocused += LogList.UnFocusLog;

            LogList.ShowCurrentClicked += () =>
            {
                Solver.ShowCurrent();
                ExplanationBox.Text = "";
            };
            LogList.ShowStartClicked += () =>
            {
                Solver.ShowStartState();
               ExplanationBox.Text = "";
            };
            LogList.LogClicked += log =>
            {
                Solver.ShowLog(log);
                ExplanationBox.Text = log.Explanation;
            };

            LiveModification.LiveModified += (number, row, col, action) =>
            {
                if (action == SolverNumberType.Definitive) Solver.AddDefinitiveNumber(number, row, col);
                else if(action == SolverNumberType.Possibility) Solver.RemovePossibility(number, row, col);
            };
            
            StrategyList.InitStrategies(Solver.GetStrategies());
            StrategyList.StrategyExcluded += Solver.ExcludeStrategy;
            StrategyList.StrategyUsed += Solver.UseStrategy;

            DelaySlider.Value = Solver.Delay;
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            if (_createNewSudoku) Solver.NewSudoku(new Sudoku(SudokuStringBox.Text));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e)
        {
            SolveButton.IsEnabled = false;
            ClearButton.IsEnabled = false;

            SolverUserControl suc = Solver;
            
            if (StepByStepOption.IsChecked == true) suc.RunUntilProgress();
            else suc.SolveSudoku();
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            SolverUserControl suc = Solver;
            suc.ClearSudoku();
        }
        
        private void SelectedTranslationType(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox box) return;
            Solver?.SetTranslationType((SudokuTranslationType) box.SelectedIndex);
        }
        
        private void SetSolverDelay(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is not Slider slider) return;
            Solver.Delay = (int)slider.Value;
        }
    }
