using Model.Solver;

namespace Model;

public interface IStrategy
{
    public string Name { get; }
    public StrategyLevel Difficulty { get; }
    public int Score { get; set; }

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