using Model.Solver.Helpers;

namespace Model.Solver;

public abstract class AbstractStrategy : IStrategy
{ 
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public abstract OnCommitBehavior DefaultOnCommitBehavior { get; }
    public StatisticsTracker Tracker { get; } = new();

    protected AbstractStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
        OnCommitBehavior = defaultBehavior;
    }
    
    public abstract void Apply(IStrategyManager strategyManager);
    public virtual void OnNewSudoku(Sudoku s) { }
    
}

public abstract class OriginalBoardBasedAbstractStrategy : AbstractStrategy
{
    protected Sudoku OriginalBoard { get; private set; } = new();
    
    protected OriginalBoardBasedAbstractStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior)
        : base(name, difficulty, defaultBehavior) { }

    public override void OnNewSudoku(Sudoku s)
    {
        base.OnNewSudoku(s);
        OriginalBoard = s.Copy();
    }
}