using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.Kakuros;
using DesktopApplication.View.Nonograms;
using DesktopApplication.View.Sudokus;
using DesktopApplication.View.Tectonics;

namespace DesktopApplication.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class WelcomeWindow
{
    private readonly WelcomePresenter _presenter;
    
    public WelcomeWindow()
    {
        InitializeComponent();

        _presenter = GlobalApplicationPresenter.Instance.InitializeWelcomePresenter();
        
        RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Fant);
            
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }

    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void OnSudokuClick(object sender, RoutedEventArgs e)
    {
        var window = new SudokuWindow();
        window.Show();
        Close();
    }
    
    private void OnTectonicClick(object sender, RoutedEventArgs e)
    {
        var window = new TectonicWindow();
        window.Show();
        Close();
    }
    
    private void OnKakuroClick(object sender, RoutedEventArgs e)
    {
        var window = new KakuroWindow();
        window.Show();
        Close();
    }
    
    private void OnNonogramClick(object sender, RoutedEventArgs e)
    {
        var window = new NonogramWindow();
        window.Show();
        Close();
    }

    private void ShowSettingWindow()
    {
        var window = new SettingWindow(_presenter.SettingsPresenter);
        window.Show();
    }
}