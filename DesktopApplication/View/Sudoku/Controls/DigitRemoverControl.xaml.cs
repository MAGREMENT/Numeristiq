using System.Windows.Media;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class DigitRemoverControl
{
    public DigitRemoverControl()
    {
        InitializeComponent();
    }

    public void Activate(bool activated)
    {
        ActivatedLamp.Background = activated ? Brushes.ForestGreen : Brushes.Red;
    }
}