using System.Windows;
using System.Windows.Controls;
using DesktopApplication.Presenter;
using DesktopApplication.View.Settings;
using Model.Core.Settings;
using Model.Utility.Collections;

namespace DesktopApplication.View.HelperWindows;

public partial class SettingWindow
{
    private readonly SettingsPresenter _presenter;
    
    public SettingWindow(SettingsPresenter presenter)
    {
        InitializeComponent();

        _presenter = presenter;

        var style = (Style)FindResource("SettingTitleRadioButton");
        int count = 0;
        foreach (var list in presenter)
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
        SettingPanel.Children.Clear();
        foreach (var settingAndIndex in settings.EnumerateWithIndex())
        {
            var control = SettingTranslator.Translate(_presenter, settingAndIndex.Item1, settingAndIndex.Item2);
            if (control is not null)
            {
                control.Margin = new Thickness(10, 10, 0, 0);
                control.MouseEnter += (_, _) => Description.Text = settingAndIndex.Item1.Description;
                control.MouseLeave += (_, _) => Description.Text = string.Empty;
                SettingPanel.Children.Add(control);
            }
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

        _presenter.Update();
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
}