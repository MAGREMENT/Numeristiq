namespace Model.Sudoku.Solver.Trackers;

public abstract class Tracker
{
    public virtual void Prepare(SudokuSolver solver)
    {
        
    }

    public virtual void OnStrategyStart(SudokuStrategy strategy, int index)
    {
        
    }

    public virtual void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        
    }

    public virtual void OnSolveDone(ISolveResult result)
    {
        
    }
}

public interface ISolveResult
{
    public IReadOnlySudoku Sudoku { get; }

    public bool IsWrong();
}