using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Model.Core.Settings;
using Model.Sudokus.Generator;

namespace DesktopApplication.View.Controls;

public partial class EvaluationCriteriaControl
{
    public event OnClick? Clicked;
    
    public EvaluationCriteriaControl(EvaluationCriteria criteria)
    {
        InitializeComponent();

        Name.Text = criteria.Name;

        int row = 1;
        foreach (var setting in criteria.Settings)
        {
            Grid.RowDefinitions.Add(new RowDefinition
            {
                Height = GridLength.Auto
            });
            
            var tb = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(20, 5, 0, 0)
            };

            var r1 = new Run
            {
                Text = setting.Name + " : "
            };
            
            r1.SetResourceReference(ForegroundProperty, "Text");
            tb.Inlines.Add(r1);

            var r2 = new Run
            {
                Text = setting.Get().ToString()
            };
            
            r2.SetResourceReference(ForegroundProperty, "Primary1");
            tb.Inlines.Add(r2);

            Grid.SetRow(tb, row++);
            Grid.Children.Add(tb);
        }
    }

    public void UpdateSettings(IReadOnlyList<IReadOnlySetting> settings)
    {
        Grid.Children.RemoveRange(1, Grid.Children.Count - 1);
        
        int row = 1;
        foreach (var setting in settings)
        {
            var tb = new TextBlock
            {
                FontSize = 14,
                Margin = new Thickness(20, 5, 0, 0)
            };

            var r1 = new Run
            {
                Text = setting.Name + " : "
            };
            
            r1.SetResourceReference(ForegroundProperty, "Text");
            tb.Inlines.Add(r1);

            var r2 = new Run
            {
                Text = setting.Get().ToString()
            };
            
            r2.SetResourceReference(ForegroundProperty, "Primary1");
            tb.Inlines.Add(r2);

            Grid.SetRow(tb, row++);
            Grid.Children.Add(tb);
        }
    }

    private void InvokeEvent(object sender, MouseButtonEventArgs e)
    {
        Clicked?.Invoke();
    }
}