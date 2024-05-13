using Model.Helpers.Changes;
using Model.Sudokus.Solver;

namespace Model.Kakuros;

public abstract class KakuroStrategy : ICommitMaker
{
    public string Name { get; }
    public StepDifficulty Difficulty { get; }
    public InstanceHandling InstanceHandling { get; }
    
    protected KakuroStrategy(string name, StepDifficulty difficulty, InstanceHandling instanceHandling)
    {
        Name = name;
        Difficulty = difficulty;
        InstanceHandling = instanceHandling;
    }

    public abstract void Apply(IKakuroStrategyUser strategyUser);
}