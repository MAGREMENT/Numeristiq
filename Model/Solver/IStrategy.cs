using Model.Solver.Helpers;

namespace Model.Solver;

public interface IStrategy //TODO : Add return after first instance found
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    public StatisticsTracker Tracker { get; }

    void ApplyOnce(IStrategyManager strategyManager);
}

public enum StrategyLevel
{
    None, Basic, Easy, Medium, Hard, Extreme, ByTrial
}

public interface IOriginalBoardNeededStrategy : IStrategy
{
    public void SetOriginalBoard(Sudoku board);
}