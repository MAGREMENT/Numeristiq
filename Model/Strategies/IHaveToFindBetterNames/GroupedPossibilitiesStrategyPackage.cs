namespace Model.Strategies.IHaveToFindBetterNames;

public class GroupedPossibilitiesStrategyPackage : SolverStrategyPackage
{
    public GroupedPossibilitiesStrategyPackage() : base(new RowDeductionStrategy(), 
        new ColumnDeductionStrategy(), new MiniGridDeductionStrategy()) { }
}