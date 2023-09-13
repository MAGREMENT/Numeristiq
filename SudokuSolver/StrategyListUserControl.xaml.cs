using Model;
using Model.Solver.Helpers;

namespace SudokuSolver;

public partial class StrategyListUserControl
{
    public delegate void OnStrategyExclusion(int number);
    public event OnStrategyExclusion? StrategyExcluded;

    public delegate void OnStrategyUse(int number);
    public event OnStrategyUse? StrategyUsed;
    
    public StrategyListUserControl()
    {
        InitializeComponent();
    }

    public void InitStrategies(StrategyInfo[] strategies)
    {
        for (int i = 0; i < strategies.Length; i++)
        {
            StrategyUserControl suc = new StrategyUserControl();
            suc.InitStrategy(strategies[i]);
            
            int a = i;
            suc.Excluded += () => StrategyExcluded?.Invoke(a);
            suc.Used += () => StrategyUsed?.Invoke(a);
            
            List.Children.Add(suc);
        }
    }
}