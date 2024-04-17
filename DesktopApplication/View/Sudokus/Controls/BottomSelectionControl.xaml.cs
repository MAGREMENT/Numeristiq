using System.Windows;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class BottomSelectionControl
{
    public event SwapPage? PageSwapped;
    
    public BottomSelectionControl()
    {
        InitializeComponent();
    }

    private void SwapTo0(object sender, RoutedEventArgs e)
    {
        PageSwapped?.Invoke(0);
    }
    
    private void SwapTo1(object sender, RoutedEventArgs e)
    {
        PageSwapped?.Invoke(1);
    }
    
    private void SwapTo2(object sender, RoutedEventArgs e)
    {
        PageSwapped?.Invoke(2);
    }
    
    private void SwapTo3(object sender, RoutedEventArgs e)
    {
        PageSwapped?.Invoke(3);
    }
}

public delegate void SwapPage(int number);