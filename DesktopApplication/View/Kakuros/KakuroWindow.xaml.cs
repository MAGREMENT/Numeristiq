using System.Windows;
using DesktopApplication.Presenter;
using DesktopApplication.View.Kakuros.Pages;

namespace DesktopApplication.View.Kakuros;

public partial class KakuroWindow
{
    public KakuroWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        var presenter = GlobalApplicationPresenter.Instance.InitializeKakuroApplicationPresenter();

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