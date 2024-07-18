using System.Windows;

namespace ThemeCreator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public new static App Current => (App)Application.Current;
}