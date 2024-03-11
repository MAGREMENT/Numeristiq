using System.Windows;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Tectonic;
using DesktopApplication.View.Tectonic.Pages;

namespace DesktopApplication.View.Tectonic;

public partial class TectonicWindow
{
    private readonly TectonicApplicationPresenter _presenter;
    
    public TectonicWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        _presenter = GlobalApplicationPresenter.Instance.InitializeTectonicApplicationPresenter();

        Frame.Content = new SolvePage(_presenter);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}