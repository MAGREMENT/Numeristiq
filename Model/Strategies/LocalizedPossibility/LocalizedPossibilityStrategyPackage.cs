namespace Model.Strategies.LocalizedPossibility;

public class LocalizedPossibilityStrategyPackage : StrategyPackage
{
    public LocalizedPossibilityStrategyPackage() : base(new RowLocalizedPossibilityStrategy(), 
        new ColumnLocalizedPossibilityStrategy(), new MiniGridLocalizedPossibilityStrategy()) { }
}