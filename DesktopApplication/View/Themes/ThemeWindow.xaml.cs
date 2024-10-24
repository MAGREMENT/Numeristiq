using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DesktopApplication.Presenter;
using DesktopApplication.Presenter.Themes;
using DesktopApplication.View.HelperWindows;
using DesktopApplication.View.HelperWindows.Dialog;
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

        _presenter = PresenterFactory.Instance.Initialize(this);
    }
    
    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    public void SetCurrentTheme(string name, bool editable)
    {
        CurrentName.Text = name;
        RemoveButton.IsEnabled = editable;
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

    public void SetContinuousUpdate(bool yes)
    {
        CurrentColorValue.ContinuousUpdate = yes;
    }

    public void RedrawExampleGrid()
    {
        Board.Refresh();
    }

    private void Copy(object sender, RoutedEventArgs e)
    {
        var dialog = new NameChooserDialog(_presenter);
        dialog.NameChosen += _presenter.SaveNewTheme;
        dialog.Show();
    }

    private void ChangeColor(RGB color)
    {
        _presenter.SetCurrentColor(color);
    }
    
    private void ToPrimary1(object sender, MouseEventArgs e)
    {
        if(sender is FrameworkElement fe) fe.SetResourceReference(BackgroundProperty, "Primary");
    }
    
    private void ToPrimary2(object sender, MouseEventArgs e)
    {
        if(sender is FrameworkElement fe) fe.SetResourceReference(BackgroundProperty, "PrimaryHighlighted");
    }
    
    private void ToSecondary1(object sender, MouseEventArgs e)
    {
        if(sender is FrameworkElement fe) fe.SetResourceReference(BackgroundProperty, "Secondary");
    }

    private void ToSecondary2(object sender, MouseEventArgs e)
    {
        if(sender is FrameworkElement fe) fe.SetResourceReference(BackgroundProperty, "SecondaryHighlighted");
    }

    private void OpenSettings()
    {
        var window = new SettingWindow(_presenter.SettingsPresenter);
        window.Show();
    }

    private void Remove(object sender, RoutedEventArgs e)
    {
        _presenter.Remove();
    }
}