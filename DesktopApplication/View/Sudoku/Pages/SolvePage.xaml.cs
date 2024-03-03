using System.Windows;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Solve;
using DesktopApplication.View.Sudoku.Controls;
using Model;
using Model.Helpers.Logs;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage : ISudokuSolveView
{
    private readonly SudokuSolvePresenter _presenter;

    private int _logOpen = -1;
    
    public SolvePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    #region ISudokuSolveView

    public ISudokuDrawer Drawer => Board;

    public void SetSudokuAsString(string s)
    {
        SudokuAsString.SetText(s);
    }

    public void DisableSolveActions()
    {
        SolveButton.IsEnabled = false;
        AdvanceButton.IsEnabled = false;
        ChooseStepButton.IsEnabled = false;
        ClearButton.IsEnabled = false;
    }

    public void EnableSolveActions()
    {
        SolveButton.IsEnabled = true;
        AdvanceButton.IsEnabled = true;
        ChooseStepButton.IsEnabled = true;
        ClearButton.IsEnabled = true;
    }

    public void AddLog(ISolverLog log, StateShown stateShown)
    {
        LogPanel.Dispatcher.Invoke(() =>
        {
            var lc = new LogControl(log, stateShown);
            LogPanel.Children.Add(lc);
            lc.OpenRequested += _presenter.RequestLogOpening;
            lc.StateShownChanged += _presenter.RequestStateShownChange;
            lc.HighlightShifted += _presenter.RequestHighlightShift;
        });
        LogViewer.Dispatcher.Invoke(() => LogViewer.ScrollToEnd());
    }

    public void ClearLogs()
    {
        LogPanel.Children.Clear();
    }

    public void OpenLog(int index)
    {
        if (index < 0 || index > LogPanel.Children.Count) return;
        if (LogPanel.Children[index] is not LogControl lc) return;

        _logOpen = index;
        lc.Open();
    }

    public void CloseLogs()
    {
        if (_logOpen < 0 || _logOpen > LogPanel.Children.Count) return;
        if (LogPanel.Children[_logOpen] is not LogControl lc) return;

        _logOpen = -1;
        lc.Close();
    }

    public void SetLogsStateShown(StateShown stateShown)
    {
        foreach (var child in LogPanel.Children)
        {
            if (child is not LogControl lc) continue;

            lc.SetStateShown(stateShown);
        }
    }

    public void SetCursorPosition(int index, string s)
    {
        if (_logOpen < 0 || _logOpen > LogPanel.Children.Count) return;
        if (LogPanel.Children[_logOpen] is not LogControl lc) return;

        lc.SetCursorPosition(s);
    }

    #endregion

    private void SetNewSudoku(string s)
    {
        _presenter.SetNewSudoku(s);
    }

    private void SudokuTextBoxShowed()
    {
        
        _presenter.OnSudokuAsStringBoxShowed();
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void ChooseStep(object sender, RoutedEventArgs e)
    {
        
    }

    private void Clear(object sender, RoutedEventArgs e)
    {
        _presenter.Clear();
    }
}

