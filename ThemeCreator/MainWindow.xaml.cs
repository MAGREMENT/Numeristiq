using System.Collections.Generic;
using System.Windows;
using DesktopApplication;
using Model.Utility;

namespace ThemeCreator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : IMainView
{
    private readonly Presenter _presenter;
    
    public MainWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        _presenter = new Presenter(this);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    public void SetColors(IEnumerable<(string, RGB)> colors)
    {
        foreach (var color in colors)
        {
            ColorList.Children.Add(new ColorControl(ThemeInformation.ToBrush(color.Item2), color.Item1));
        }
    }
}