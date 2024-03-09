using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.View.Controls;
using Model.Helpers.Settings;
using Model.Utility;

namespace DesktopApplication.View.HelperWindows;

public partial class SettingWindow
{
    private readonly SettingsSpan _span;
    
    public SettingWindow(SettingsSpan span)
    {
        InitializeComponent();

        _span = span;

        var style = ((App)Application.Current).Resources["SettingTitleStyle"] as Style;
        int count = 0;
        foreach (var list in span)
        {
            var rb = new RadioButton
            {
                Content = list.Name,
                Style = style,
            };
            rb.Checked += (_, _) => ShowSettings(list);

            if (count++ == 0) rb.IsChecked = true;
            TitlePanel.Children.Add(rb);
        }
        
        TitleBar.RefreshMaximizeRestoreButton(WindowState);
        StateChanged += (_, _) => TitleBar.RefreshMaximizeRestoreButton(WindowState);
    }

    private void ShowSettings(NamedListSpan<ISetting> settings)
    {
        foreach (var setting in settings)
        {
            SettingControl? control = setting.InteractionInterface switch
            {
                NameListInteractionInterface => new NameListControl(setting),
                _ => null
            };

            if (control is not null) SettingPanel.Children.Add(control);
        }
    }

    private void Minimize()
    {
        WindowState = WindowState.Minimized;
    }

    private void ChangeSize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Save(object sender, RoutedEventArgs e)
    {
        foreach (var item in SettingPanel.Children)
        {
            if (item is not SettingControl sc) continue;
            sc.Set();
        }

        _span.Update();
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public abstract class SettingControl : UserControl
{
    public bool AutoSet { get; set; }
    public abstract void Set();
    
    protected ISetting Setting { get; }

    protected SettingControl(ISetting setting)
    {
        Setting = setting;
    }
}