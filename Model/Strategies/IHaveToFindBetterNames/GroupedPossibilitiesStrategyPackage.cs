namespace Model.Strategies.IHaveToFindBetterNames;

public class GroupedPossibilitiesStrategyPackage : StrategyPackage
{
    public GroupedPossibilitiesStrategyPackage() : base(new RowDeductionStrategy(), 
        new ColumnDeductionStrategy(), new MiniGridDeductionStrategy()) { }
}