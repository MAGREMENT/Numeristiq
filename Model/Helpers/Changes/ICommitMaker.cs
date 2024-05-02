using Model.Sudokus.Solver;

namespace Model.Helpers.Changes;

public interface ICommitMaker
{
    public string Name { get; }
    public StepDifficulty Difficulty { get; }
    public InstanceHandling InstanceHandling { get; }
}