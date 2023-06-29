namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityStrategyPackage : ISolverStrategyPackage
{
    private readonly ISolverStrategy[] _strategies = {new RowSinglePossibilityStrategy(),
        new ColumnSinglePossibilityStrategy(), new MiniGridSinglePossibilityStrategy(), 
        new CellSinglePossibilityStrategy()
    };
    
    public bool ApplyOnce(ISolver solver)
    {
        bool wasProgressMade = false;
        foreach (var strategy in _strategies)
        {
            if (strategy.ApplyOnce(solver)) wasProgressMade = true;
        }

        return wasProgressMade;
    }
}