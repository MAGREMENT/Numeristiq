using Model.Helpers.Changes;
using Model.Sudoku.Solver;

namespace Model.Tectonic;

public abstract class AbstractTectonicStrategy : ICommitMaker
{
    public string Name { get; }
    public StrategyDifficulty Difficulty { get; }
    public OnCommitBehavior OnCommitBehavior { get; set; }

    protected AbstractTectonicStrategy(string name, StrategyDifficulty difficulty, OnCommitBehavior defaultBehavior)
    {
        Name = name;
        Difficulty = difficulty;
        OnCommitBehavior = defaultBehavior;
    }
    
    public abstract void Apply(IStrategyUser strategyUser);
}