using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class DigitRemoverControl
{
    public event OnSymmetryChange? SymmetryChanged;
    public event OnUniqueChange? UniqueChanged;
    
    public DigitRemoverControl()
    {
        InitializeComponent();
    }

    public void Activate(bool activated)
    {
        ActivatedLamp.SetResourceReference(BackgroundProperty, activated ? "On" : "Off");
    }

    private void ToSymmetric(object sender, RoutedEventArgs e)
    {
        SymmetryChanged?.Invoke(true);
    }

    private void ToNonSymmetric(object sender, RoutedEventArgs e)
    {
        SymmetryChanged?.Invoke(false);
    }

    private void ToUnique(object sender, RoutedEventArgs e)
    {
        UniqueChanged?.Invoke(true);
    }

    private void ToNonUnique(object sender, RoutedEventArgs e)
    {
        UniqueChanged?.Invoke(false);
    }
}

public delegate void OnSymmetryChange(bool value);

public delegate void OnUniqueChange(bool value);