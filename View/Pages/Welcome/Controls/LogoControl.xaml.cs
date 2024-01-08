using System.Windows;
using View.Themes;

namespace View.Pages.Welcome.Controls;

public partial class LogoControl
{
    public double Size
    {
        set
        {
            First.FontSize = value;
            Second.FontSize = value;
            Second.Margin = new Thickness(value / 2 * 3, 0, 0, 0);
        }
    }
    
    public LogoControl()
    {
        InitializeComponent();
    }
}