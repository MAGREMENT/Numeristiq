using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Themes;
using DesktopApplication.View.Themes.Controls;
using Model.Repositories;
using Model.Utility;

namespace DesktopApplication.View.Themes;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ThemeWindow : IThemeView
{
    private readonly ThemePresenter _presenter;
    
    public ThemeWindow()
    {
        InitializeComponent();
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);

        _presenter = GlobalApplicationPresenter.Instance.InitializeThemePresenter(this);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    public void SetCurrentTheme(string name)
    {
        CurrentName.Text = name + " Theme";
    }

    public void SetOtherThemes(IEnumerable<(Theme, bool)> themes)
    {
        OtherThemes.Children.Clear();
        foreach (var t in themes)
        {
            var control = new ThemeControl(t.Item1.Name, t.Item2);
            control.MouseLeftButtonDown += (_, _) => _presenter.SetTheme(t.Item1.Name);

            OtherThemes.Children.Add(control);
        }
    }

    public void SetColors(IEnumerable<(string, RGB)> colors)
    {
        ColorList.Children.Clear();
        foreach (var color in colors)
        {
            var control = new ColorControl(ThemeInformation.ToBrush(color.Item2), color.Item1);
            control.MouseLeftButtonDown += (_, _) => _presenter.SelectColor(control.ColorName.Text);
            ColorList.Children.Add(control);
        }
    }

    public void SelectColor(string name)
    {
        CurrentColor.SetResourceReference(ForegroundProperty, "Text");
        CurrentColor.Text = name;
    }

    public void UnselectColor()
    {
        CurrentColor.SetResourceReference(ForegroundProperty, "Disabled");
        CurrentColor.Text = "None";
    }
}