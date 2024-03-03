using System.Windows;
using DesktopApplication.View.Tectonic.Pages;

namespace DesktopApplication.View.Tectonic;

public partial class TectonicWindow
{
    public TectonicWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        Frame.Content = new SolvePage();
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