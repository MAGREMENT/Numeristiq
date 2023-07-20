namespace Model.Strategies;

public class AvoidableRectangleStrategy : IStrategy
{
    public string Name { get; } = "Avoidable rectangle";
    
    public StrategyLevel Difficulty { get; } = StrategyLevel.Hard;
    
    public AvoidableRectangleStrategy(Sudoku initialState)
    {
    }
    
    public void ApplyOnce(ISolverView solverView)
    {
        throw new System.NotImplementedException();
        
    }
}