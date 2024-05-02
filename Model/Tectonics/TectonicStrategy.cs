using Model.Helpers.Changes;
using Model.Sudokus.Solver;

namespace Model.Tectonics;

public abstract class TectonicStrategy : ICommitMaker
{
    public string Name { get; }
    public StepDifficulty Difficulty { get; }
    public InstanceHandling InstanceHandling { get; set; }
    
    protected bool StopOnFirstPush => InstanceHandling == InstanceHandling.FirstOnly;

    protected TectonicStrategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling)
    {
        Name = name;
        Difficulty = difficulty;
        InstanceHandling = defaultHandling;
    }
    
    public abstract void Apply(ITectonicStrategyUser strategyUser);
}