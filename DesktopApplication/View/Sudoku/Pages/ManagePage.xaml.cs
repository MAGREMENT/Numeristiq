using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Manage;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class ManagePage : ISudokuManageView
{
    private readonly SudokuManagePresenter _presenter;
    public ManagePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }
}