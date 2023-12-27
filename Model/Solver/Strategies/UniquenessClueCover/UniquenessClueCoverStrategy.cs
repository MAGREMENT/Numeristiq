namespace Model.Solver.Strategies.UniquenessClueCover;

public class UniquenessClueCoverStrategy : AbstractStrategy
{
    public const string OfficialName = "Uniqueness Clue Cover";
    private const OnCommitBehavior DefaultBehavior = OnCommitBehavior.Return;

    public override OnCommitBehavior DefaultOnCommitBehavior => DefaultBehavior;

    private readonly IPatternCollection[] _collections;
    
    public UniquenessClueCoverStrategy(params IPatternCollection[] collections)
        : base(OfficialName, StrategyDifficulty.Hard, DefaultBehavior)
    {
        _collections = collections;
    }

    
    public override void Apply(IStrategyManager strategyManager)
    {
        foreach (var c in _collections)
        {
            if (c.Apply(strategyManager)) return;
        }
    }
}