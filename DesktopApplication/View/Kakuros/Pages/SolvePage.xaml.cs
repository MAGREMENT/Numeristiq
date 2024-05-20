using System.Windows;
using System.Windows.Input;
using DesktopApplication.Presenter.Kakuros;
using DesktopApplication.Presenter.Kakuros.Solve;
using DesktopApplication.View.Kakuros.Controls;
using Model.Utility;

namespace DesktopApplication.View.Kakuros.Pages;

public partial class SolvePage : IKakuroSolveView
{
    private readonly KakuroSolvePresenter _presenter;

    public IKakuroSolverDrawer Drawer => (KakuroBoard)ContentControl.OptimizableContent!;
    
    public SolvePage(KakuroApplicationPresenter presenter)
    {
        InitializeComponent();

        _presenter = presenter.Initialize(this);
    }


    private void OnNewKakuro(string s)
    {
        _presenter.SetNewKakuro(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void OnCellSelection(Cell cell, bool isAmountCell)
    {
        if (isAmountCell) _presenter.SelectSum(cell.Row, cell.Column);
        else _presenter.SelectCell(cell.Row, cell.Column);
    }

    private void DoBoardInput(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.A : _presenter.AddCell();
                break;
            case Key.Z : _presenter.RemoveCell();
                break;
            case Key.NumPad1 : _presenter.AddDigitToAmount(1);
                break;
            case Key.NumPad2 : _presenter.AddDigitToAmount(2);
                break;
            case Key.NumPad3 : _presenter.AddDigitToAmount(3);
                break;
            case Key.NumPad4 : _presenter.AddDigitToAmount(4);
                break;
            case Key.NumPad5 : _presenter.AddDigitToAmount(5);
                break;
            case Key.NumPad6 : _presenter.AddDigitToAmount(6);
                break;
            case Key.NumPad7 : _presenter.AddDigitToAmount(7);
                break;
            case Key.NumPad8 : _presenter.AddDigitToAmount(8);
                break;
            case Key.NumPad9 : _presenter.AddDigitToAmount(9);
                break;
            case Key.Back : _presenter.RemoveDigitFromAmount();
                break;
            case Key.Enter : _presenter.EnterAmount();
                break;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        _presenter.EnterAmount();
    }
}