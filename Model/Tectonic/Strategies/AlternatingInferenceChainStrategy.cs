using Model.Sudoku.Solver;

namespace Model.Tectonic.Strategies;

public class AlternatingInferenceChainStrategy : TectonicStrategy
{
    public AlternatingInferenceChainStrategy() : base("Alternating Inference Chain", StrategyDifficulty.Extreme, InstanceHandling.BestOnly)
    {
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                foreach (var p in strategyUser.PossibilitiesAt(row, col).Enumerate(
                             1, strategyUser.Tectonic.GetZone(row, col).Count))
                {
                    return; //TODO with linkgraph
                }
            }
        }
    }
}