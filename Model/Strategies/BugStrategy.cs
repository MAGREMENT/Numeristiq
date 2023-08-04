namespace Model.Strategies;

public class BugStrategy : IStrategy
{
    public string Name { get; } = "BUG";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Medium;
    public int Score { get; set; }

    public void ApplyOnce(IStrategyManager strategyManager)
    {
        int[]? triple = OnlyDoublesAndOneTriple(strategyManager);
        if (triple is not null)
        {
            foreach (var possibility in strategyManager.Possibilities[triple[0], triple[1]])
            {
                if (strategyManager.PossibilityPositionsInColumn(triple[1], possibility).Count % 2 == 1 &&
                    strategyManager.PossibilityPositionsInRow(triple[0], possibility).Count % 2 == 1 &&
                    strategyManager.PossibilityPositionsInMiniGrid(triple[0] / 3, triple[1] / 3, possibility).Count % 2 == 1)
                {
                    strategyManager.AddDefinitiveNumber(possibility, triple[0], triple[1], this);
                }
            }
        }
    }

    public string GetExplanation(IChangeCauseFactory factory)
    {
        return "";
    }

    private int[]? OnlyDoublesAndOneTriple(IStrategyManager strategyManager)
    {
        int[]? triple = null;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (strategyManager.Sudoku[row, col] == 0 && strategyManager.Possibilities[row, col].Count != 2)
                {
                    if (strategyManager.Possibilities[row, col].Count != 3 || triple is not null) return null;
                    triple = new[] { row, col };
                }
            }
        }

        return triple;
    }
}