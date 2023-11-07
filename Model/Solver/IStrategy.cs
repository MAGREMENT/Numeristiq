using Model.Solver.Helpers;

namespace Model.Solver;

public interface IStrategy
{ 
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public UniquenessDependency UniquenessDependency { get; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
    public OnCommitBehavior DefaultOnCommitBehavior { get; }
    public StatisticsTracker Tracker { get; }
    
    void Apply(IStrategyManager strategyManager);
    void OnNewSudoku(Sudoku s);
}

public enum StrategyDifficulty
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}

public enum UniquenessDependency
{
    NotDependent, PartiallyDependent, FullyDependent
}

public enum OnCommitBehavior
{
    Return, WaitForAll, ChooseBest
}