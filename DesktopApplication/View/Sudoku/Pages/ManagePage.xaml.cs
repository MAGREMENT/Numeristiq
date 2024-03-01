using System.Windows.Controls;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Manage;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class ManagePage : Page
{
    private readonly SudokuManagePresenter _presenter;
    public ManagePage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.ManagePresenter;
    }
}