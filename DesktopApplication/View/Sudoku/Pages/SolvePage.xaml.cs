using System.Windows;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Solve;
using DesktopApplication.View.Sudoku.Controls;
using Model.Helpers;
using Model.Helpers.Logs;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage : ISudokuSolveView
{
    private readonly SudokuSolvePresenter _presenter;
    
    public SolvePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    #region ISudokuSolveView

    public void SetSudokuAsString(string s)
    {
        SudokuAsString.SetText(s);
    }

    public void DisplaySudoku(ITranslatable translatable)
    {
        Board.Dispatcher.Invoke(() => Board.Show(translatable));
    }

    public void SetClues(ITranslatable translatable)
    {
        Board.SolutionsToSpecialBrush(translatable);
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

    public void AddLog(ISolverLog log)
    {
        LogPanel.Dispatcher.Invoke(() => LogPanel.Children.Add(new LogControl(log)));
        LogViewer.Dispatcher.Invoke(() => LogViewer.ScrollToEnd());
    }

    public void ClearLogs()
    {
        LogPanel.Children.Clear();
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