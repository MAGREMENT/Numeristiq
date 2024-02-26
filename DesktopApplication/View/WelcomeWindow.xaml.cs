using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopApplication.View.Sudoku;

namespace DesktopApplication.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class WelcomeWindow
{
    public WelcomeWindow()
    {
        InitializeComponent();
        
        RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.Fant);
        RenderOptions.SetBitmapScalingMode(GameImage, BitmapScalingMode.Fant);
            
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

    private void OnSudokuMouseEnter(object sender, MouseEventArgs e)
    {
        GameImage.Source = new BitmapImage(new Uri("/Images/SudokuImage.png", UriKind.Relative));
    }

    private void OnTectonicMouseEnter(object sender, MouseEventArgs e)
    {
        GameImage.Source = new BitmapImage(new Uri("/Images/TectonicImage.png", UriKind.Relative));
    }

    private void OnSudokuClick(object sender, RoutedEventArgs e)
    {
        var window = new SudokuWindow();
        window.Show();
        Close();
    }
}