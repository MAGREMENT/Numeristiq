using System.Windows;
using System.Windows.Media;
using DesktopApplication.Presenter;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.Sudokus.Pages;
using DesktopApplication.View.Themes;
using DesktopApplication.View.YourPuzzles.Pages;
using SolvePage = DesktopApplication.View.Tectonics.Pages.SolvePage;

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

        _presenter = PresenterFactory.Instance.Initialize();
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
        var window = new PageWindow(1400, 750, new Sudokus.Pages.SolvePage(),
            new PlayPage(), new ManagePage(), new GeneratePage());
        window.Show();
        Close();
    }
    
    private void OnTectonicClick(object sender, RoutedEventArgs e)
    {
        var window = new PageWindow(1210, 700, new SolvePage());
        window.Show();
        Close();
    }
    
    private void OnKakuroClick(object sender, RoutedEventArgs e)
    {
        var window = new PageWindow(1050, 800, new Kakuros.Pages.SolvePage());
        window.Show();
        Close();
    }
    
    private void OnNonogramClick(object sender, RoutedEventArgs e)
    {
        var window = new PageWindow(1060, 700, new Nonograms.Pages.SolvePage());
        window.Show();
        Close();
    }
    
    private void OnBinairoClick(object sender, RoutedEventArgs e)
    {
        var window = new PageWindow(1050, 700, new Binairos.Pages.SolvePage());
        window.Show();
        Close();
    }
    
    private void OnNumeBoardClick(object sender, RoutedEventArgs e)
    {
        var window = new PageWindow(1300, 720, new CreatePage());
        window.Show();
        Close();
    }

    private void ShowSettingWindow()
    {
        var window = new SettingWindow(_presenter.SettingsPresenter);
        window.Show();
    }

    private void ShowThemeWindow(object sender, RoutedEventArgs e)
    {
        var window = new ThemeWindow();
        window.Show();
    }
}