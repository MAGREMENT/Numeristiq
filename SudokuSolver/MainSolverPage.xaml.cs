using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Solver;
using SudokuSolver.Pages;
using SudokuSolver.SolverOptions;
using SudokuSolver.Utils;

namespace SudokuSolver;

public partial class MainSolverPage : IGraphicsManager, ISolverOptionHandler
{
    public event OnTranslationTypeChanged? TranslationTypeChanged;
        
    private bool _createNewSudoku = true;

    public MainSolverPage(IPageHandler pageHandler)
    {
        InitializeComponent();

        Navigation.PageHandler = pageHandler;
        Navigation.AddCustomButton("Options", () =>
        {
            var optionWindow = new SolverOptionWindow(this);
            optionWindow.Show();
        });

        var unused = new SolverStateManager(this, Solver, LogList);

        Solver.IsReady += () =>
        {
            SolveButton.IsEnabled = true;
            ClearButton.IsEnabled = true;
        };
        Solver.CellClickedOn += (sender, row, col) =>
        {
            //TODO
        };
        Solver.SudokuAsStringChanged += ShowSudokuAsString;
        Solver.LogsUpdated += logs =>
        {
            LogList.Dispatcher.Invoke(() => LogList.InitLogs(logs));
            ExplanationBox.Dispatcher.Invoke(() => ExplanationBox.Text = "");
        };
        Solver.CurrentStrategyChanged += index =>
            StrategyList.Dispatcher.Invoke(() => StrategyList.HighlightStrategy(index));

        StrategyList.InitStrategies(Solver.GetStrategies());
        StrategyList.StrategyExcluded += Solver.ExcludeStrategy;
        StrategyList.StrategyUsed += Solver.UseStrategy;
    }
    
    public void ShowSudokuAsString(string asString)
    {
        _createNewSudoku = false;
        SudokuStringBox.Text = asString;
        _createNewSudoku = true;
    }

    public void ShowExplanation(string explanation)
    { 
        ExplanationBox.Text = explanation;
    }

    private void NewSudoku(object sender, TextChangedEventArgs e)
    {
        if (_createNewSudoku) Solver.NewSudoku(SudokuTranslator.Translate(SudokuStringBox.Text));
    }

    private void SolveSudoku(object sender, RoutedEventArgs e)
    {
        SolveButton.IsEnabled = false;
        ClearButton.IsEnabled = false;

        SolverUserControl suc = Solver;
            
        if (StepByStep) suc.RunUntilProgress();
        else suc.SolveSudoku();
    }

    private void ClearSudoku(object sender, RoutedEventArgs e)
    {
        SolverUserControl suc = Solver;
        suc.ClearSudoku();
    }

    //ISolverOptionHandler----------------------------------------------------------------------------------------------

    public int DelayBefore
    {
        get => Solver.DelayBefore;

        set => Solver.DelayBefore = value;
    }

    public int DelayAfter
    {
        get => Solver.DelayAfter;

        set => Solver.DelayAfter = value;
    }

    public SudokuTranslationType TranslationType
    {
        get => Solver.TranslationType;
        set
        {
            Solver.TranslationType = value;
            TranslationTypeChanged?.Invoke(); 
        }
    }

    public bool StepByStep { get; set; } = true;

    public bool UniquenessAllowed
    {
        get => Solver.UniquenessAllowed;
        set
        {
            Solver.AllowUniqueness(value);
            StrategyList.UpdateStrategies(Solver.GetStrategies());
        }
    }

    public OnInstanceFound OnInstanceFound
    {
        get => Solver.OnInstanceFound;
        set => Solver.SetOnInstanceFound(value);
    }
}