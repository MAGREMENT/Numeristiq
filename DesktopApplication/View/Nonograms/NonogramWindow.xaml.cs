using System.Windows;
using DesktopApplication.Presenter;
using DesktopApplication.View.Nonograms.Pages;

namespace DesktopApplication.View.Nonograms;

public partial class NonogramWindow
{
    public NonogramWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        var presenter = GlobalApplicationPresenter.Instance.InitializeNonogramApplicationPresenter();

        Frame.Content = new SolvePage(presenter);
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