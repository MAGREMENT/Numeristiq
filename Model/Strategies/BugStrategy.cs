namespace Model.Strategies;

public class BugStrategy : IStrategy
{
    public string Name { get; } = "BUG";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(ISolverView solverView)
    {
        int[]? triple = OnlyDoublesAndOneTriple(solverView);
        if (triple is not null)
        {
            foreach (var possibility in solverView.Possibilities[triple[0], triple[1]])
            {
                if (solverView.PossibilityPositionsInColumn(triple[1], possibility).Count % 2 == 1 &&
                    solverView.PossibilityPositionsInRow(triple[0], possibility).Count % 2 == 1 &&
                    solverView.PossibilityPositionsInMiniGrid(triple[0] / 3, triple[1] / 3, possibility).Count % 2 == 1)
                {
                    solverView.AddDefinitiveNumber(possibility, triple[0], triple[1], this);
                }
            }
        }
    }

    private int[]? OnlyDoublesAndOneTriple(ISolverView solverView)
    {
        int[]? triple = null;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (solverView.Sudoku[row, col] == 0 && solverView.Possibilities[row, col].Count != 2)
                {
                    if (solverView.Possibilities[row, col].Count != 3 || triple is not null) return null;
                    triple = new[] { row, col };
                }
            }
        }

        return triple;
    }
}