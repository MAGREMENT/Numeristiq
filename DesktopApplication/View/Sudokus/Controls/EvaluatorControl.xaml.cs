using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DesktopApplication.View.Controls;
using Model.Sudokus.Generator;

namespace DesktopApplication.View.Sudokus.Controls;

public partial class EvaluatorControl
{
    public event OnManageCriteriaAsked? ManageCriteriaAsked;
    
    public EvaluatorControl()
    {
        InitializeComponent();
    }
    
    public void Activate(bool activated)
    {
        ActivatedLamp.SetResourceReference(BackgroundProperty, activated ? "On" : "Off");
    }

    private void ManageCriteria(object sender, RoutedEventArgs e)
    {
        ManageCriteriaAsked?.Invoke();
    }
    
    public void SetCriteriaList(IReadOnlyList<EvaluationCriteria> criteriaList)
    {
        CriteriaList.Children.Clear();
        foreach (var criteria in criteriaList)
        {
            CriteriaList.Children.Add(new EvaluationCriteriaControl(criteria));
        }
    }
}

public delegate void OnManageCriteriaAsked();