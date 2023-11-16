using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Global;
using Global.Enums;
using Presenter;
using Presenter.Translator;
using View.Settings;
using View.Utils;

namespace View.Pages.Solver;

public partial class SolverPage : ISolverView, ISolverOptionHandler
{
    private bool _createNewSudoku = true;

    private readonly SolverPresenter _presenter;

    public SolverPage(IPageHandler pageHandler)
    {
        InitializeComponent();

        _presenter = SolverPresenter.FromView(this);
        _presenter.Bind();
        
        Navigation.PageHandler = pageHandler;
        Navigation.AddCustomButton("Settings", () =>
        {
            var settingsWindow = new SolverSettingsWindow(this);
            settingsWindow.Show();
        });

        LogList.LogSelected += _presenter.SelectLog;
        LogList.ShowStartStateAsked += _presenter.ShowStartState;
        LogList.ShowCurrentStateAsked += _presenter.ShowCurrentState;
        StrategyList.StrategyUsed += _presenter.UseStrategy;
    }
    
    //ISolverView-------------------------------------------------------------------------------------------------------

    public void SetCellTo(int row, int col, int number)
    {
        Solver.SetCellTo(row, col, number);
    }

    public void SetCellTo(int row, int col, int[] possibilities)
    {
        Solver.SetCellTo(row, col, possibilities);
    }

    public void UpdateGivens(HashSet<Cell> givens)
    {
        Solver.UpdateGivens(givens);
    }

    public void SetTranslation(string translation)
    {
        _createNewSudoku = false;
        SudokuStringBox.Text = translation;
        _createNewSudoku = true;
    }

    public void FocusLog(int number)
    {
        LogList.FocusLog(number);
    }

    public void UnFocusLog()
    {
        LogList.UnFocusLog();
    }

    public void ShowExplanation(string explanation)
    { 
        ExplanationBox.Text = explanation;
    }

    public void SetLogs(IReadOnlyList<ViewLog> logs)
    {
        LogList.SetLogs(logs);
    }

    public void ClearLogs()
    {
        LogList.ClearLogs();
    }

    public void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies)
    {
        StrategyList.InitializeStrategies(strategies);
    }

    public void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies)
    {
        StrategyList.UpdateStrategies(strategies);
    }

    public void LightUpStrategy(int number)
    {
        StrategyList.Dispatcher.Invoke(() => StrategyList.LightUpStrategy(number));
    }

    //ISolverOptionHandler----------------------------------------------------------------------------------------------

    public int DelayBeforeTransition
    {
        get => _presenter.Settings.DelayBeforeTransition;

        set => _presenter.Settings.DelayBeforeTransition = value;
    }

    public int DelayAfterTransition
    {
        get => _presenter.Settings.DelayAfterTransition;

        set => _presenter.Settings.DelayAfterTransition = value;
    }

    public SudokuTranslationType TranslationType
    {
        get => _presenter.Settings.TranslationType;
        set => _presenter.Settings.TranslationType = value;
    }

    public bool StepByStep
    {
        get => _presenter.Settings.StepByStep;
        set => _presenter.Settings.StepByStep = value;
    }

    public bool UniquenessAllowed
    {
        get => _presenter.Settings.UniquenessAllowed;
        set => _presenter.Settings.UniquenessAllowed = value;
    }

    public OnInstanceFound OnInstanceFound
    {
        get => _presenter.Settings.OnInstanceFound;
        set => _presenter.Settings.OnInstanceFound = value;
    }

    public ChangeType ActionOnKeyboardInput
    {
        get => Solver.ActionOnKeyboardInput;
        set => Solver.ActionOnKeyboardInput = value;
    }

    public Brush GivenForegroundColor
    {
        get => ColorManager.GetInstance().GivenForegroundColor;
        set
        {
            ColorManager.GetInstance().GivenForegroundColor = value;
            _presenter.Settings.NotifyGivensNeedingUpdate();
        }
    }
    public Brush SolvingForegroundColor
    {
        get => ColorManager.GetInstance().SolvingForegroundColor;
        set
        {
            ColorManager.GetInstance().SolvingForegroundColor = value;
            _presenter.Settings.NotifyGivensNeedingUpdate();
        } 
    }
    
    //EventHandling-----------------------------------------------------------------------------------------------------

    private void NewSudoku(object sender, TextChangedEventArgs e)
    {
        if (_createNewSudoku) _presenter.NewSudokuFromString(SudokuStringBox.Text);
    }

    private void SolveSudoku(object sender, RoutedEventArgs e)
    {
        _presenter.Solve();
    }

    private void ClearSudoku(object sender, RoutedEventArgs e)
    {
        _presenter.ClearSudoku();
    }
}