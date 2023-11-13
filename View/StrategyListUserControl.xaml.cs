using Model.Solver;

namespace View;

public partial class StrategyListUserControl
{
    public delegate void OnStrategyExclusion(int number);
    public event OnStrategyExclusion? StrategyExcluded;

    public delegate void OnStrategyUse(int number);
    public event OnStrategyUse? StrategyUsed;

    private int _currentlyHighlighted = -1;
    
    public StrategyListUserControl()
    {
        InitializeComponent();
    }

    public void InitStrategies(StrategyInfo[] strategies)
    {
        for (int i = 0; i < strategies.Length; i++)
        {
            StrategyUserControl suc = new StrategyUserControl();
            suc.Update(strategies[i]);
            
            int a = i;
            suc.Excluded += () => StrategyExcluded?.Invoke(a);
            suc.Used += () => StrategyUsed?.Invoke(a);
            
            List.Children.Add(suc);
        }
    }

    public void UpdateStrategies(StrategyInfo[] strategies)
    {
        for (int i = 0; i < strategies.Length; i++)
        {
            ((StrategyUserControl) List.Children[i]).Update(strategies[i]);
        }
    }

    public void HighlightStrategy(int index)
    {
        if (_currentlyHighlighted != -1)
        {
            ((StrategyUserControl) List.Children[_currentlyHighlighted]).UnHighlight();
        }

        if (index != -1)
        {
            ((StrategyUserControl) List.Children[index]).Highlight();
        }

        _currentlyHighlighted = index;
    }
}