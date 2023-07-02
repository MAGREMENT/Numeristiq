namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityStrategyPackage : StrategyPackage
{
    public SinglePossibilityStrategyPackage() : base(new CellSinglePossibilityStrategy(), 
        new RowSinglePossibilityStrategy(), new ColumnSinglePossibilityStrategy(),
        new MiniGridSinglePossibilityStrategy()) { }
}