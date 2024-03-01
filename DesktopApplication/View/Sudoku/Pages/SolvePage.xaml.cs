using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Solve;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class SolvePage
{
    private readonly SudokuSolvePresenter _presenter;
    
    public SolvePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.SolvePresenter;
    }
}