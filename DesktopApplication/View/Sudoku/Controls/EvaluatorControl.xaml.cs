using System.Windows;
using System.Windows.Media;

namespace DesktopApplication.View.Sudoku.Controls;

public partial class EvaluatorControl
{
    public event OnManageCriteriaAsked? ManageCriteriaAsked;
    
    public EvaluatorControl()
    {
        InitializeComponent();
    }
    
    public void Activate(bool activated)
    {
        ActivatedLamp.Background = activated ? Brushes.ForestGreen : Brushes.Red;
    }

    private void ManageCriteria(object sender, RoutedEventArgs e)
    {
        ManageCriteriaAsked?.Invoke();
    }
}

public delegate void OnManageCriteriaAsked();