using System.Windows.Controls;
using Model;

namespace SudokuSolver;

public partial class StrategyListUserControl : UserControl
{
    private readonly StackPanel _list;

    public delegate void OnStrategyExclusion(int number);
    public event OnStrategyExclusion? StrategyExcluded;

    public delegate void OnStrategyUse(int number);
    public event OnStrategyUse? StrategyUsed;
    
    public StrategyListUserControl()
    {
        InitializeComponent();

        _list = (FindName("List") as StackPanel)!;
    }

    public void InitStrategies(IStrategy[] strategies)
    {
        for (int i = 0; i < strategies.Length; i++)
        {
            StrategyUserControl suc = new StrategyUserControl();
            suc.InitStrategy(strategies[i]);
            
            int a = i;
            suc.Excluded += () => StrategyExcluded?.Invoke(a);
            suc.Used += () => StrategyUsed?.Invoke(a);
            
            _list.Children.Add(suc);
        }
    }
}