using Model.Core;
using Model.Tectonics.Solver;

namespace Model.Tectonics;

public abstract class TectonicStrategy : Strategy
{ 
    protected TectonicStrategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling) 
        : base(name, difficulty, defaultHandling)
    {
        Name = name;
        Difficulty = difficulty;
        InstanceHandling = defaultHandling;
    }
    
    public abstract void Apply(ITectonicStrategyUser strategyUser);
}