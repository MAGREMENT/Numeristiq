using System.Windows;
using System.Windows.Controls;
using Model;

namespace SudokuSolver;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _createNewSudoku = true;

        public MainWindow() //TODO replace FindName() with properties
        {
            InitializeComponent();

            GetSolverUserControl().IsReady += () => { GetSolveButton().IsEnabled = true; };

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

            GetLogListUserControl().ShowCurrentClicked += () =>
            {
                GetSolverUserControl().ShowCurrent();
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
                GetLogListUserControl().InitLogs(GetSolverUserControl().GetLogs());
            };
            
            GetStrategyList().InitStrategies(GetSolverUserControl().GetStrategies());
            GetStrategyList().StrategyExcluded += GetSolverUserControl().ExcludeStrategy;
            GetStrategyList().StrategyUsed += GetSolverUserControl().UseStrategy;
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            if (_createNewSudoku) GetSolverUserControl().NewSudoku(new Sudoku(GetSudokuString().Text));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e) //TODO fix : update string value when run until progress
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            SolverUserControl suc = GetSolverUserControl();
            
            if (GetStepByStepOption().IsChecked == true) suc.RunUntilProgress();
            else suc.SolveSudoku();

            GetLogListUserControl().InitLogs(suc.GetLogs());
        }

        private void ClearSudoku(object sender, RoutedEventArgs e)
        {
            SolverUserControl suc = GetSolverUserControl();
            suc.ClearSudoku();
            
            GetLogListUserControl().InitLogs(suc.GetLogs());
        }

        /*Gets*/
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
