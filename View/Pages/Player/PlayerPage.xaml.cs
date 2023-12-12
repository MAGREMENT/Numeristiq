using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Global;
using Global.Enums;
using Presenter;
using Presenter.Player;

namespace View.Pages.Player;

public partial class PlayerPage : IPlayerView
{
    private readonly IPageHandler _pageHandler;
    private readonly PlayerPresenter _presenter;
    private readonly SudokuGrid _grid;
    
    public PlayerPage(IPageHandler handler, PresenterFactory factory)
    {
        InitializeComponent();

        _presenter = factory.Create(this);
        _pageHandler = handler;

        _grid = new SudokuGrid(24, 1, 3);
        Panel.Children.Add(_grid);

        _grid.CellSelected += (row, col) => _presenter.RestartSelection(row, col);
        _grid.CellAddedToSelection += _presenter.AddToSelection;
    }

    public override void OnShow()
    {
        
    }

    public override void OnQuit()
    {
        
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        _pageHandler.ShowPage(PagesName.First);
    }

    public void ShowPossibilities(int row, int col, int[] possibilities, PossibilitiesLocation location)
    {
        _grid.ShowLinePossibilities(row, col, possibilities, location, Brushes.Black);
    }

    public void ShowNumber(int row, int col, int number)
    {
        _grid.ShowSolution(row, col, number, Brushes.Black);
    }

    public void ClearNumbers()
    {
        _grid.ClearNumbers();
    }

    public void PutCursorOn(HashSet<Cell> cells)
    {
        _grid.PutCursorOn(cells);
    }

    public void ClearCursor()
    {
        _grid.ClearCursor();
    }

    public void Refresh()
    {
        _grid.Refresh();
    }
}