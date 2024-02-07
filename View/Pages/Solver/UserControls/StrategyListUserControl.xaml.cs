using System.Collections.Generic;
using System.Windows.Input;
using Presenter.Sudoku.Translators;

namespace View.Pages.Solver.UserControls;

public partial class StrategyListUserControl
{ 
    public delegate void OnStrategyUse(int number, bool used);
    public event OnStrategyUse? StrategyUsed;

    public delegate void OnAllStrategiesUse(bool used);
    public event OnAllStrategiesUse? AllStrategiesUsed;
    
    public StrategyListUserControl()
    {
        InitializeComponent();
    }
    
    public void InitializeStrategies(IReadOnlyList<ViewStrategy> strategies)
    {
        List.Children.Clear();
        
        for (int i = 0; i < strategies.Count; i++)
        {
            StrategyUserControl suc = new StrategyUserControl();
            suc.Match(strategies[i]);
            
            var iForEvent = i;
            suc.UsageChanged += (used) => StrategyUsed?.Invoke(iForEvent, used);

            List.Children.Add(suc);
        }
    }

    public void UpdateStrategies(IReadOnlyList<ViewStrategy> strategies)
    {
        if (List.Children.Count == 0) return;
        
        for (int i = 0; i < strategies.Count; i++)
        {
            ((StrategyUserControl) List.Children[i]).Match(strategies[i]);
        }
    }

    public void LightUpStrategy(int number)
    {
        var children = List.Children;
        if (number >= children.Count || number < 0) return;
        
        ((StrategyUserControl) children[number]).LightUp();
    }

    public void StopLightingUpStrategy(int number)
    {
        var children = List.Children;
        if (number >= children.Count || number < 0) return;
        
        ((StrategyUserControl) children[number]).UnLightUp();
    }

    private void UseAllStrategies(object sender, MouseButtonEventArgs e)
    {
        AllStrategiesUsed?.Invoke(true);
        foreach (var s in List.Children)
        {
            ((StrategyUserControl)s).Check(true);
        }
    }

    private void ExcludeAllStrategies(object sender, MouseButtonEventArgs e)
    {
        AllStrategiesUsed?.Invoke(false);
        foreach (var s in List.Children)
        {
            ((StrategyUserControl)s).Check(false);
        }
    }
}