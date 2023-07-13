namespace Model.Strategies;

public class BugStrategy : IStrategy
{
    public void ApplyOnce(ISolver solver)
    {
        int[]? triple = OnlyDoublesAndOneTriple(solver);
        if (triple is not null)
        {
            foreach (var possibility in solver.Possibilities[triple[0], triple[1]].All())
            {
                if (solver.PossibilityPositionsInColumn(triple[1], possibility).Count % 2 == 1 &&
                    solver.PossibilityPositionsInRow(triple[0], possibility).Count % 2 == 1 &&
                    solver.PossibilityPositionsInMiniGrid(triple[0] / 3, triple[1] / 3, possibility).Count % 2 == 1)
                {
                    solver.AddDefinitiveNumber(possibility, triple[0], triple[1],
                        new BugLog(possibility, triple[0], triple[1]));
                }
            }
        }
    }

    private int[]? OnlyDoublesAndOneTriple(ISolver solver)
    {
        int[]? triple = null;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solver.Sudoku[row, col] == 0 && solver.Possibilities[row, col].Count != 2)
                {
                    if (solver.Possibilities[row, col].Count != 3 || triple is not null) return null;
                    triple = new[] { row, col };
                }
            }
        }

        return triple;
    }
}

public class BugLog : ISolverLog
{
    public string AsString { get; }
    public StrategyLevel Level { get; } = StrategyLevel.Medium;

    public BugLog(int number, int row, int col)
    {
        AsString = $"[{row + 1}, {col + 1}] {number} added as definitive because of bug strategy";
    }
}