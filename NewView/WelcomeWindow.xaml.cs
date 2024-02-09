using System.Windows;

namespace NewView;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class WelcomeWindow
{
    public WelcomeWindow()
    {
        InitializeComponent();
            
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }

    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }
}