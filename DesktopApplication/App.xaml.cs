using DesktopApplication.Presenter;
using DesktopApplication.View.Utility;
using Model;

namespace DesktopApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public App()
    {
        InitializeComponent();

        var presenter = new GlobalApplicationPresenter();
        presenter.Initialize();
        if (presenter.Theme is not null) SetTheme(presenter.Theme);
    }

    private void SetTheme(Theme t)
    {
        Resources["Background1"] = ColorUtility.ToBrush(t.Background1);
        Resources["Background2"] = ColorUtility.ToBrush(t.Background2);
        Resources["Background3"] = ColorUtility.ToBrush(t.Background3);
        Resources["Primary1"] = ColorUtility.ToBrush(t.Primary1);
        Resources["Primary2"] = ColorUtility.ToBrush(t.Primary2);
        Resources["Secondary1"] = ColorUtility.ToBrush(t.Secondary1);
        Resources["Secondary2"] = ColorUtility.ToBrush(t.Secondary2);
        Resources["Accent"] = ColorUtility.ToBrush(t.Accent);
        Resources["Text"] = ColorUtility.ToBrush(t.Text);
        Resources["ThumbColor"] = ColorUtility.ToBrush(t.ThumbColor);
    }
}