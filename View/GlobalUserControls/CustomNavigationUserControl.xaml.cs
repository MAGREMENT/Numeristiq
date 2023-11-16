using System.Windows;
using System.Windows.Controls;

namespace View.GlobalUserControls;

public partial class CustomNavigationUserControl : UserControl
{
    public IPageHandler? PageHandler { get; set; }
    
    public CustomNavigationUserControl()
    {
        InitializeComponent();
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        PageHandler?.ShowPage(PagesName.First);
    }

    public void AddCustomButton(string name, OnButtonClick handler)
    {
        var b = new Button
        {
            Height = 25,
            Width = 75,
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Center,
            Content = name,
            Margin = new Thickness(10, 0, 0, 0)
        };

        b.Click += (_, _) => handler();

        Main.Children.Add(b);
    }
}

public delegate void OnButtonClick();