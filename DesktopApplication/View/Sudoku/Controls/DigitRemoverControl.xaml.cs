using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class DigitRemoverControl
{
    public event OnSymmetryChange? SymmetryChanged;
    
    public DigitRemoverControl()
    {
        InitializeComponent();
    }

    public void Activate(bool activated)
    {
        ActivatedLamp.Background = activated ? Brushes.ForestGreen : Brushes.Red;
    }

    private void ToSymmetric(object sender, RoutedEventArgs e)
    {
        SymmetryChanged?.Invoke(true);
    }

    private void ToNonSymmetric(object sender, RoutedEventArgs e)
    {
        SymmetryChanged?.Invoke(false);
    }
}

public delegate void OnSymmetryChange(bool value);