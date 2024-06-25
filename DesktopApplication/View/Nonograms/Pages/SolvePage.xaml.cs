using System.Windows;
using DesktopApplication.Presenter.Nonograms;
using DesktopApplication.Presenter.Nonograms.Solve;

namespace DesktopApplication.View.Nonograms.Pages;

public partial class SolvePage : INonogramSolveView
{
    private readonly NonogramSolvePresenter _presenter;

    public INonogramDrawer Drawer => (INonogramDrawer)EmbeddedDrawer.OptimizableContent!;

    public SolvePage(NonogramApplicationPresenter presenter)
    {
        InitializeComponent();

        _presenter = presenter.Initialize(this);
    }
    
    public void ShowNonogramAsString(string s)
    {
        TextBox.SetText(s);
    }

    private void Solve(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }

    private void CreateNewNonogram(string s)
    {
        _presenter.SetNewNonogram(s);
    }

    private void OnHideableTextboxShowed()
    {
        _presenter.ShowNonogramAsString();
    }
}