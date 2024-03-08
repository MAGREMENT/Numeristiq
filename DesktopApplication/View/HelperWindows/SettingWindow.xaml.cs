using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;

namespace DesktopApplication.View.HelperWindows;

public partial class SettingWindow
{
    private readonly SettingsSpan _span;
    
    public SettingWindow(SettingsSpan span)
    {
        InitializeComponent();
        
        _span = span;

        var style = ((App)Application.Current).Resources["SettingTitleStyle"] as Style;
        foreach (var list in span)
        {
            TitlePanel.Children.Add(new RadioButton()
            {
                Content = list.Name,
                Style = style
            });
        }
        
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
}