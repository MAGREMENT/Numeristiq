namespace Model.Solver.Strategies.UniquenessClueCover;

public class UniquenessClueCoverStrategy : AbstractStrategy
{
    public const string OfficialName = "Uniqueness Clue Cover";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IPatternCollection[] _collections;
    private bool _needFilter = true;
    
    public UniquenessClueCoverStrategy(params IPatternCollection[] collections)
        : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _collections = collections;
        foreach (var c in _collections)
        {
            c.Strategy = this;
        }
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        foreach (var c in _collections)
        {
            if (_needFilter)
            {
                if (c.Filter(strategyManager)) return;
            }
            else if (c.Apply(strategyManager)) return;
        }

        _needFilter = false;
    }

    public override void OnNewSudoku(Sudoku s)
    {
        _needFilter = true;
    }
}