namespace Model.Strategies.IntersectionRemoval;

public class IntersectionRemovalStrategyPackage : StrategyPackage
{
    public IntersectionRemovalStrategyPackage() : base(new RowBoxLineReductionStrategy(), 
        new ColumnBoxLineReductionStrategy(), new PointingPossibilitiesStrategy()) { }
}