using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;

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
                    if (Search(strategyUser, new CellPossibility(row, col, p))) return;
                }
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, CellPossibility start)
    {
        HashSet<CellPossibility> on = new();
        Dictionary<CellPossibility, CellPossibility> off = new();
        Queue<CellPossibility> queue = new();

        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
        }
        
        return false;
    }

    private IEnumerable<CellPossibility> GetOn(IStrategyUser strategyUser, CellPossibility cp)
    {
        var sameCell = strategyUser.PossibilitiesAt(cp.Row, cp.Column);
        var zone = strategyUser.Tectonic.GetZone(cp.Row, cp.Column);

        if (sameCell.Count == 2) yield return new CellPossibility(cp.Row, cp.Column,
                sameCell.First(1, zone.Count, cp.Possibility));
        
        //TODO
    }
}