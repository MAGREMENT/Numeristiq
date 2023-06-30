namespace Model.Strategies.SinglePossibility;

public class SinglePossibilityStrategyPackage : SolverStrategyPackage
{
    public SinglePossibilityStrategyPackage() : base(new CellSinglePossibilityStrategy(), 
        new RowSinglePossibilityStrategy(), new ColumnSinglePossibilityStrategy(),
        new MiniGridSinglePossibilityStrategy()) { }
}