namespace Model.Sudoku.Solver.Trackers;

public class RatingTracker : Tracker
{
    private double total;
    private int count;

    public double Rating => total / count;
    
    public override void OnStrategyEnd(SudokuStrategy strategy, int index, int solutionAdded, int possibilitiesRemoved)
    {
        if (solutionAdded + possibilitiesRemoved == 0) return;

        count++;
        total += (int)strategy.Difficulty;
    }

    public void Clear()
    {
        total = 0;
        count = 0;
    }
}