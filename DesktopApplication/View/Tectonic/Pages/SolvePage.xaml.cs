using System.Windows;
using DesktopApplication.Presenter.Tectonic;
using DesktopApplication.Presenter.Tectonic.Solve;

namespace DesktopApplication.View.Tectonic.Pages;

public partial class SolvePage : ITectonicSolveView
{
    private readonly TectonicSolvePresenter _presenter;
    
    public SolvePage(TectonicApplicationPresenter appPresenter)
    {
        InitializeComponent();

        _presenter = appPresenter.Initialize(this);
    }

    public ITectonicDrawer Drawer => EmbeddedDrawer.Drawer;

    private void CreateNewTectonic(string s)
    {
        _presenter.SetNewTectonic(s);
    }

    private void Solver(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(false);
    }

    private void Advance(object sender, RoutedEventArgs e)
    {
        _presenter.Solve(true);
    }
}