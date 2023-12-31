using System.Windows;
using System.Windows.Controls;
using View.Canvas;

namespace View.HelperWindows.Settings;

public partial class SettingsPage : Page
{
    public string SettingTitle { get; }

    public delegate void OnExplanationToBeShown(string explanation);
    public event OnExplanationToBeShown? ExplanationToBeShown;
    
    public SettingsPage(string title, params OptionCanvas[] options)
    {
        InitializeComponent();

        SettingTitle = title;

        foreach (var option in options)
        {
            option.Margin = new Thickness(10, 10, 0, 0);
            option.MouseEnter += (_, _) => ExplanationToBeShown?.Invoke(option.Explanation);
            option.MouseLeave += (_, _) => ExplanationToBeShown?.Invoke("");
            Panel.Children.Add(option);
        }
    }

    public void Refresh()
    {
        foreach (OptionCanvas option in Panel.Children)
        {
            option.Refresh();
        }
    }
}