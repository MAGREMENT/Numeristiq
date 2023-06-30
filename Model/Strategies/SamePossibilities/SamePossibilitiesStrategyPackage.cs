namespace Model.Strategies.SamePossibilities;

public class SamePossibilitiesStrategyPackage : SolverStrategyPackage
{
    public SamePossibilitiesStrategyPackage() : base(new RowSamePossibilitiesStrategy(),
        new ColumnSamePossibilitiesStrategy(), new MiniGridSamePossibilitiesStrategy()) { }
}