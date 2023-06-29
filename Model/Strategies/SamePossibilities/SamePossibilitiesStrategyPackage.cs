namespace Model.Strategies.SamePossibilities;

public class SamePossibilitiesStrategyPackage : ISolverStrategyPackage
{
    private readonly ISolverStrategy[] _strategies = {new RowSamePossibilitiesStrategy(),
        new ColumnSamePossibilitiesStrategy(), new MiniGridSamePossibilitiesStrategy()};
    
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