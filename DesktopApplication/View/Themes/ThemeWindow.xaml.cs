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
        _presenter.EvaluateName(string.Empty);
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

    public void SetColors(IEnumerable<(string, RGB)> colors, bool canBeSelected)
    {
        ColorList.Children.Clear();
        foreach (var color in colors)
        {
            var control = new ColorControl(ThemeInformation.ToBrush(color.Item2), color.Item1, canBeSelected);
            control.MouseLeftButtonDown += (_, _) => _presenter.SelectColor(control.ColorName.Text);
            ColorList.Children.Add(control);
        }
    }

    public void SelectColor(string name, RGB value)
    {
        CurrentColorName.SetResourceReference(ForegroundProperty, "Text");
        CurrentColorName.Text = name;
        CurrentColorValue.Color = value;
    }

    public void UnselectColor()
    {
        CurrentColorName.SetResourceReference(ForegroundProperty, "Disabled");
        CurrentColorName.Text = "None";
        CurrentColorValue.NoColor();
    }

    public void ShowNameError(string error)
    {
        NameFeedback.Text = error;
        NameFeedback.SetResourceReference(ForegroundProperty, "Off");
        SaveAsButton.IsEnabled = false;
    }

    public void ShowNameIsCorrect()
    {
        NameFeedback.Text = "This name is valid";
        NameFeedback.SetResourceReference(ForegroundProperty, "On");
        SaveAsButton.IsEnabled = true;
    }

    private void EvaluateName(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox box) return;
        _presenter.EvaluateName(box.Text);
    }

    private void SaveAs(object sender, RoutedEventArgs e)
    {
        _presenter.SaveNewTheme(SaveAsName.Text);
    }

    private void ChangeColor(RGB color)
    {
        _presenter.SetCurrentColor(color);
    }
}