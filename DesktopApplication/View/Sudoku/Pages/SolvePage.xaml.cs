using System.Windows;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Solve;
using Model.Helpers;

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
        _presenter.FullSolve();
    }
}