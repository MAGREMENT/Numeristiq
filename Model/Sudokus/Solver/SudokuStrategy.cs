using Model.Core;
using Model.Core.Settings;

namespace Model.Sudokus.Solver;

public abstract class SudokuStrategy : Strategy<ISudokuSolverData>
{ 
    protected SudokuStrategy(string name, StepDifficulty difficulty, InstanceHandling defaultHandling) 
        : base(name, difficulty, defaultHandling) { }
    
    public virtual void OnNewSudoku(IReadOnlySudoku s) { }

    public override bool Equals(object? obj)
    {
        return obj is SudokuStrategy ss && ss.Name.Equals(Name);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}