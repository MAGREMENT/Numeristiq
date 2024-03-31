using System.Windows.Input;
using DesktopApplication.Presenter.Sudoku;
using DesktopApplication.Presenter.Sudoku.Play;
using DesktopApplication.View.Controls;
using DesktopApplication.View.HelperWindows;
using Model.Utility;

namespace DesktopApplication.View.Sudoku.Pages;

public partial class PlayPage : ISudokuPlayView
{
    private readonly SudokuPlayPresenter _presenter;
    public PlayPage(SudokuApplicationPresenter appPresenter)
    {
        InitializeComponent();
        _presenter = appPresenter.Initialize(this);
    }

    #region ISudokuPlayView

    public ISudokuPlayerDrawer Drawer => Board;

    #endregion

    public override void OnShow()
    {
        
    }

    public override void OnClose()
    {
       
    }

    public override object TitleBarContent()
    {
        var settings = new SettingsButton();
        settings.Clicked += () =>
        {
            var window = new SettingWindow(_presenter.SettingsPresenter);
            window.Show();
        };

        return settings;
    }

    private void SelectCell(int row, int col)
    {
        _presenter.SelectCell(new Cell(row, col));
    }

    private void AddCellToSelection(int row, int col)
    {
        _presenter.AddCellToSelection(new Cell(row, col));
    }
    
    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.D1 :
            case Key.NumPad1 :
                _presenter.ActOnCurrentCells(1);
                break;
            case Key.D2 :
            case Key.NumPad2 :
                _presenter.ActOnCurrentCells(2);
                break;
            case Key.D3 :
            case Key.NumPad3 :
                _presenter.ActOnCurrentCells(3);
                break;
            case Key.D4 :
            case Key.NumPad4 :
                _presenter.ActOnCurrentCells(4);
                break;
            case Key.D5 :
            case Key.NumPad5 :
                _presenter.ActOnCurrentCells(5);
                break;
            case Key.D6 :
            case Key.NumPad6 :
                _presenter.ActOnCurrentCells(6);
                break;
            case Key.D7 :
            case Key.NumPad7 :
                _presenter.ActOnCurrentCells(7);
                break;
            case Key.D8 :
            case Key.NumPad8 :
                _presenter.ActOnCurrentCells(8);
                break;
            case Key.D9 :
            case Key.NumPad9 :
                _presenter.ActOnCurrentCells(9);
                break;
            case Key.D0 :
            case Key.NumPad0 :
            case Key.Back :
                _presenter.RemoveCurrentCells();
                break;
        }
    }
}