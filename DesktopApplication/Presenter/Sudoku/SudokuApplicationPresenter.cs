using DesktopApplication.Presenter.Sudoku.Generate;
using DesktopApplication.Presenter.Sudoku.Manage;
using DesktopApplication.Presenter.Sudoku.Play;
using DesktopApplication.Presenter.Sudoku.Solve;

namespace DesktopApplication.Presenter.Sudoku;

public class SudokuApplicationPresenter
{
    public SudokuSolvePresenter SolvePresenter { get; } = new();
    public SudokuPlayPresenter PlayPresenter { get; } = new();
    public SudokuManagePresenter ManagePresenter { get; } = new();
    public SudokuGeneratePresenter GeneratePresenter { get; } = new();
}