using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication;
using Model.Repositories;
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

    public void SetCurrentTheme(Theme theme)
    {
        App.Current.SetTheme(theme);
        CurrentName.Text = theme.Name + " Theme";
    }

    public void SetOtherThemes(IEnumerable<Theme> themes)
    {
        foreach (var t in themes)
        {
            var tb = new TextBlock
            {
                FontSize = 14,
                Padding = new Thickness(5),
                Text = t.Name
            };
            tb.SetResourceReference(ForegroundProperty, "Text");

            OtherThemes.Children.Add(tb);
        }
    }

    public void SetColors(IEnumerable<(string, RGB)> colors)
    {
        foreach (var color in colors)
        {
            ColorList.Children.Add(new ColorControl(ThemeInformation.ToBrush(color.Item2), color.Item1));
        }
    }
}