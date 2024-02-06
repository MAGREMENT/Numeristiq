using Model.Sudoku.Solver;

namespace Model.Helpers.Changes;

public interface ICommitMaker
{
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public OnCommitBehavior OnCommitBehavior { get; set; }
}