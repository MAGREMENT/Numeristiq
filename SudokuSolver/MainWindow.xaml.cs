using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Logs;

namespace SudokuSolver;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly StackPanel _mainPanel;
        private readonly StackPanel _optionsPanel;
        private readonly StackPanel _modificationsPanel;
        private readonly StackPanel _asidePanel;
        private readonly StackPanel _asidePanel2;

        private bool _createNewSudoku = true;

        public MainWindow()
        {
            InitializeComponent();

            _mainPanel = (FindName("MainPanel") as StackPanel)!;
            _optionsPanel = (FindName("OptionsPanel") as StackPanel)!;
            _modificationsPanel = (FindName("ModificationsPanel") as StackPanel)!;
            _asidePanel = (FindName("AsidePanel") as StackPanel)!;
            _asidePanel2 = (FindName("AsidePanelTwo") as StackPanel)!;

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
            if (_createNewSudoku) GetSolverUserControl().NewSolver(
                new Solver(new Sudoku(GetSudokuString().Text)));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e) //TODO fix : update string value when solve
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            SolverUserControl suc = GetSolverUserControl();

            bool? stepByStep = GetStepByStepOption().IsChecked;
            if (stepByStep is null || (bool) !stepByStep) suc.SolveSudoku();
            else suc.RunUntilProgress();

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
            return (SolverUserControl)_mainPanel.Children[0];
        }

        private LiveModificationUserControl GetLiveModificationUserControl()
        {
            return (LiveModificationUserControl)_modificationsPanel.Children[1];
        }

        private LogListUserControl GetLogListUserControl()
        {
            return (LogListUserControl)_asidePanel.Children[0];
        }

        private Button GetSolveButton()
        {
            return (Button)((StackPanel)_mainPanel.Children[2]).Children[0];
        }

        private TextBox GetSudokuString()
        {
            return (TextBox)_mainPanel.Children[1];
        }

        private CheckBox GetStepByStepOption()
        {
            return (CheckBox)_optionsPanel.Children[1];
        }

        private TextBox GetExplanationBox()
        {
            return (TextBox)_asidePanel.Children[1];
        }

        private StrategyListUserControl GetStrategyList()
        {
            return (StrategyListUserControl)_asidePanel2.Children[0];
        }
    }
