namespace Model.DeprecatedStrategies.SamePossibilities;

public class SamePossibilitiesStrategyPackage : StrategyPackage
{
    public SamePossibilitiesStrategyPackage() : base(new RowSamePossibilitiesStrategy(),
        new ColumnSamePossibilitiesStrategy(), new MiniGridSamePossibilitiesStrategy()) { }
}