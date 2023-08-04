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

        private bool _createNewSudoku = true;

        public MainWindow()
        {
            InitializeComponent();

            _mainPanel = (FindName("MainPanel") as StackPanel)!;
            _optionsPanel = (FindName("OptionsPanel") as StackPanel)!;
            _modificationsPanel = (FindName("ModificationsPanel") as StackPanel)!;
            _asidePanel = (FindName("AsidePanel") as StackPanel)!;

            GetSudokuUserControl().IsReady += () => { GetSolveButton().IsEnabled = true; };

            GetSudokuUserControl().CellClickedOn += (sender, row, col) =>
            {
                GetLiveModificationUserControl().SetCurrent(sender, row, col);
            };

            GetSudokuUserControl().SolverUpdated += asString =>
            {
                _createNewSudoku = false;
                GetSudokuString().Text = asString;
                _createNewSudoku = true;
            };

            GetLogListUserControl().ShowCurrentClicked += () =>
            {
                GetSudokuUserControl().ShowCurrent();
                GetExplanationBox().Text = "";
            };
            GetLogListUserControl().LogClicked += log =>
            {
                GetSudokuUserControl().ShowLog(log);
                GetExplanationBox().Text = log.Explanation;
            };
            
            GetLiveModificationUserControl().LiveModified += (number, row, col, action) =>
            {
                if (action == SolverNumberType.Definitive) GetSudokuUserControl().AddDefinitiveNumber(number, row, col);
                else if(action == SolverNumberType.Possibility) GetSudokuUserControl().RemovePossibility(number, row, col);
                GetLogListUserControl().InitLogs(GetSudokuUserControl().GetLogs());
            };
        }

        private void NewSudoku(object sender, TextChangedEventArgs e)
        {
            if (_createNewSudoku) GetSudokuUserControl().NewSolver(
                new Solver(new Sudoku(GetSudokuString().Text)));
        }

        private void SolveSudoku(object sender, RoutedEventArgs e) //TODO fix : update string value when solve
        {
            if (sender is not Button butt) return;
            butt.IsEnabled = false;

            SudokuUserControl suc = GetSudokuUserControl();

            bool? stepByStep = GetStepByStepOption().IsChecked;
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
            return (SudokuUserControl)_mainPanel.Children[0];
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
    }
