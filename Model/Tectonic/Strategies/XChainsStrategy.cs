using System.Collections.Generic;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Tectonic.Strategies;

public class XChainsStrategy : TectonicStrategy
{
    public XChainsStrategy() : base("X-Chains", StrategyDifficulty.Hard, OnCommitBehavior.ChooseBest)
    {
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        var linkGraph = ConstructLinkGraph(strategyUser);

        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var zone = strategyUser.Tectonic.GetZone(row, col);
                foreach (var p in strategyUser.PossibilitiesAt(row, col).Enumerate(1, zone.Count))
                {
                    
                }
            }
        }
    }

    private ILinkGraph<CellPossibility> ConstructLinkGraph(IStrategyUser strategyUser)
    {
        var result = new DictionaryLinkGraph<CellPossibility>();

        //Neighbors
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var zone = strategyUser.Tectonic.GetZone(row, col);
                foreach (var p in strategyUser.PossibilitiesAt(row, col).Enumerate(1, zone.Count))
                {
                    List<CellPossibility> buffer = new();
                    foreach (var n in strategyUser.Tectonic.GetNeighbors(row, col))
                    {
                        if (strategyUser.PossibilitiesAt(n).Contains(p)) buffer.Add(new CellPossibility(n, p));
                    }

                    var linkStrength = buffer.Count == 1 ? LinkStrength.Strong : LinkStrength.Weak;
                    var current = new CellPossibility(row, col, p);
                    
                    foreach (var cp in buffer)
                    {
                        result.Add(current, cp, linkStrength);
                    }
                }
            }
        }

        for (int i = 0; i < strategyUser.Tectonic.Zones.Count; i++)
        {
            var zone = strategyUser.Tectonic.Zones[i];
            for (int n = 1; n <= zone.Count; n++)
            {
                var cells = strategyUser.ZonePositionsFor(i, n).ToArray();
                var linkStrength = cells.Length == 2 ? LinkStrength.Strong : LinkStrength.Weak;

                for (int a = 0; a < cells.Length - 1; a++)
                {
                    for (int b = a + 1; b < cells.Length; b++)
                    {
                        result.Add(new CellPossibility(zone[cells[a]], n), new CellPossibility(zone[cells[b]], n), linkStrength);
                    }
                }
            }
        }

        return result;
    }
}