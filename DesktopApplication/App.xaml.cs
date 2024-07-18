using System.Windows;
using DesktopApplication.Presenter;

namespace DesktopApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : IGlobalApplicationView
{
    public new static App Current => (App)Application.Current;
    
    public App()
    {
        InitializeComponent();

        GlobalApplicationPresenter.InitializeInstance(this);
        FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
        {
            DefaultValue = FindResource(typeof(Window))
        });
    }
}

