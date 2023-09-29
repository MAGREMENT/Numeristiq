using Model.Solver.Helpers;

namespace Model.Solver;

public abstract class AbstractStrategy : IStrategy
{ 
    public string Name { get; protected init; }
    public StrategyDifficulty Difficulty { get; protected init; }
    public UniquenessDependency UniquenessDependency { get; protected init; }
    public StatisticsTracker Tracker { get; } = new();

    protected AbstractStrategy(string name, StrategyDifficulty difficulty)
    {
        Name = name;
        Difficulty = difficulty;
        UniquenessDependency = UniquenessDependency.NotDependent;
    }
    
    public abstract void ApplyOnce(IStrategyManager strategyManager);
    public virtual void OnNewSudoku(Sudoku s) { }
    
}

public abstract class OriginalBoardBasedAbstractStrategy : AbstractStrategy
{
    public Sudoku OriginalBoard { get; private set; } = new();
    
    protected OriginalBoardBasedAbstractStrategy(string name, StrategyDifficulty difficulty) : base(name, difficulty)
    {
    }

    public override void OnNewSudoku(Sudoku s)
    {
        base.OnNewSudoku(s);
        OriginalBoard = s;
    }
}