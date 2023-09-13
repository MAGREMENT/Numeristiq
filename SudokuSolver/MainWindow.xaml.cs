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

            GetSolverUserControl().IsReady += () =>
            {
                GetSolveButton().IsEnabled = true;
                GetClearButton().IsEnabled = true;
            };
            GetSolverUserControl().CellClickedOn += (sender, row, col) =>
            {
                GetLiveModificationUserControl().SetCurrent(sender, row, col);
            };
            GetSolverUserControl().SolverUpdated += asString =>
            {
                _createNewSudoku = false;
                GetSudokuString().Text = asString;
                _createNewSudoku = true;
            };
            GetSolverUserControl().LogsUpdated += logs =>
            {
                GetLogListUserControl().InitLogs(logs);
                GetExplanationBox().Text = "";
            };

            GetLogListUserControl().ShowCurrentClicked += () =>
            {
                GetSolverUserControl().ShowCurrent();
                GetExplanationBox().Text = "";
            };
            GetLogListUserControl().ShowStartClicked += () =>
            {
                GetSolverUserControl().ShowStartState();
                GetExplanationBox().Text = "";
            };
            GetLogListUserControl().LogClicked += log =>
            {
                GetSolverUserControl().ShowLog(log);
                GetExplanationBox().Text = log.Explanation;
            };

            GetLiveModificationUserControl().LiveModified += (number, row, col, action) =>
            {
                if (action == SolverNumberType.Definitive) GetSolverUserControl().AddDefinitiveNumber(number, row, col);
                else if(action == SolverNumberType.Possibility) GetSolverUserControl().RemovePossibility(number, row, col);
            };
            
            GetStrategyList().InitStrategies(GetSolverUserControl().GetStrategies());
            GetStrategyList().StrategyExcluded += GetSolverUserControl().ExcludeStrategy;
            GetStrategyList().StrategyUsed += GetSolverUserControl().UseStrategy;

            DelaySlider.Value = GetSolverUserControl().Delay;
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            if (_createNewSudoku) GetSolverUserControl().NewSudoku(new Sudoku(GetSudokuString().Text));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e)
        {
            GetSolveButton().IsEnabled = false;
            GetClearButton().IsEnabled = false;

            SolverUserControl suc = GetSolverUserControl();
            
            if (GetStepByStepOption().IsChecked == true) suc.RunUntilProgress();
            else suc.SolveSudoku();
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            SolverUserControl suc = GetSolverUserControl();
            suc.ClearSudoku();
        }
        
        private void SelectedTranslationType(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox box) return;
            if (MainPanel is null) return;
            GetSolverUserControl().SetTranslationType((SudokuTranslationType) box.SelectedIndex);
        }
        
        private void SetSolverDelay(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is not Slider slider) return;
            GetSolverUserControl().Delay = (int)slider.Value;
        }

        //Gets => Should probably make properties but i'm lazy
        private SolverUserControl GetSolverUserControl()
        {
            return (SolverUserControl)MainPanel.Children[0];
        }

        private LiveModificationUserControl GetLiveModificationUserControl()
        {
            return (LiveModificationUserControl)ModificationsPanel.Children[1];
        }

        private LogListUserControl GetLogListUserControl()
        {
            return (LogListUserControl)AsidePanel.Children[0];
        }

        private Button GetSolveButton()
        {
            return (Button)((StackPanel)MainPanel.Children[2]).Children[0];
        }
        
        private Button GetClearButton()
        {
            return (Button)((StackPanel)MainPanel.Children[2]).Children[1];
        }

        private TextBox GetSudokuString()
        {
            return (TextBox)MainPanel.Children[1];
        }

        private CheckBox GetStepByStepOption()
        {
            return (CheckBox)OptionsPanel.Children[1];
        }

        private TextBox GetExplanationBox()
        {
            return (TextBox)AsidePanel.Children[1];
        }

        private StrategyListUserControl GetStrategyList()
        {
            return (StrategyListUserControl)AsidePanelTwo.Children[0];
        }
    }
