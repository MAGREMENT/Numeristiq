namespace Model.Sudoku.Solver;

public class RatingTracker
{
    private double total = 0;
    private int count = 0;

    public double Rating => total / count;

    public RatingTracker(SudokuSolver solver)
    {
        var info = solver.GetStrategyInfo();
        solver.StrategyStopped += (i, a, p) =>
        {
            if (a + p == 0) return;

            count++;
            total += (int)info[i].Difficulty;
        };
    }

    public void Clear()
    {
        total = 0;
        count = 0;
    }
}