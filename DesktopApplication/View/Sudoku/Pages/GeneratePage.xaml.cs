using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Generate;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class GeneratePage : ISudokuGenerateView
{
    private readonly SudokuGeneratePresenter _presenter;
    public GeneratePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }
}