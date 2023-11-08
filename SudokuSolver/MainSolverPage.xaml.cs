using System.Windows;
using System.Windows.Controls;
using Model;
using Model.Solver;
using Model.Solver.Helpers.Changes;
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
            LiveModification.SetCurrent(sender, row, col);
        };
        Solver.SudokuAsStringChanged += ShowSudokuAsString;
        Solver.LogsUpdated += logs =>
        {
            LogList.Dispatcher.Invoke(() => LogList.InitLogs(logs));
            ExplanationBox.Dispatcher.Invoke(() => ExplanationBox.Text = "");
        };
        Solver.CurrentStrategyChanged += index =>
            StrategyList.Dispatcher.Invoke(() => StrategyList.HighlightStrategy(index));

        LiveModification.LiveModified += (number, row, col, action) =>
        {
            if (action == ChangeType.Solution) Solver.AddDefinitiveNumber(number, row, col);
            else if(action == ChangeType.Possibility) Solver.RemovePossibility(number, row, col);
        };
            
        StrategyList.InitStrategies(Solver.GetStrategies());
        StrategyList.StrategyExcluded += Solver.ExcludeStrategy;
        StrategyList.StrategyUsed += Solver.UseStrategy;

        DelayBeforeSlider.Value = Solver.DelayBefore;
        DelayAfterSlider.Value = Solver.DelayAfter;
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
        
    private void SelectedTranslationType(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox box || Solver is null) return;
        Solver.TranslationType = (SudokuTranslationType) box.SelectedIndex;
        TranslationTypeChanged?.Invoke();
    }
        
    private void SelectedOnInstanceFound(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox box || Solver is null) return;
        Solver.SetOnInstanceFound((OnInstanceFound) box.SelectedIndex);
    }
        
    private void SetSolverDelayBefore(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        Solver.DelayBefore = (int)slider.Value;
    }
        
    private void SetSolverDelayAfter(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider slider) return;
        Solver.DelayAfter = (int)slider.Value;
    }

    private void AllowUniqueness(object sender, RoutedEventArgs e)
    {
        if (Solver is null) return;
        Solver.AllowUniqueness(true);
        StrategyList.UpdateStrategies(Solver.GetStrategies());
    }

    private void ForbidUniqueness(object sender, RoutedEventArgs e)
    {
        Solver.AllowUniqueness(false); 
        StrategyList.UpdateStrategies(Solver.GetStrategies());
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