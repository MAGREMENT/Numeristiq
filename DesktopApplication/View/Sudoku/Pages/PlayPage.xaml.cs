using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Play;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class PlayPage : ISudokuPlayView
{
    private readonly SudokuPlayPresenter _presenter;
    public PlayPage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    public override void OnShow()
    {
        
    }

    public override void OnClose()
    {
       
    }

    public override object? TitleBarContent()
    {
        return null;
    }
}