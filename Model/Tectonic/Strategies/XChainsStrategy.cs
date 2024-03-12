using System;
using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Tectonic.Strategies;

public class XChainsStrategy : TectonicStrategy //TODO Correct this dumb shit
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
                    if(Search(strategyUser, linkGraph, new CellPossibility(row, col, p))) return;
                }
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, ILinkGraph<CellPossibility> graph, CellPossibility start)
    {
        Dictionary<CellPossibility, CellPossibility> onParent = new();
        Dictionary<CellPossibility, CellPossibility> offParent = new();
        Queue<(CellPossibility, LinkStrength)> queue = new();

        foreach (var friend in graph.Neighbors(start, LinkStrength.Strong))
        {
            onParent.Add(friend, start);
            queue.Enqueue((friend, LinkStrength.Any));
        }

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            foreach (var friend in graph.Neighbors(next.Item1, next.Item2))
            {
                if (next.Item2 == LinkStrength.Strong)
                {
                    if (onParent.ContainsKey(friend)) continue;

                    onParent.Add(friend, next.Item1);
                    queue.Enqueue((friend, LinkStrength.Any));
                    if (TryEliminate(strategyUser, start, friend)) return true;
                }
                else
                {
                    if (offParent.ContainsKey(friend)) continue;

                    offParent.Add(friend, next.Item1);
                    queue.Enqueue((friend, LinkStrength.Strong));
                }
            }
        }
        
        return false;
    }

    private bool TryEliminate(IStrategyUser strategyUser, CellPossibility one, CellPossibility two)
    {
        var c1 = one.ToCell();
        var c2 = two.ToCell();
        
        foreach (var cell in Cells.SharedNeighboringCells(strategyUser.Tectonic, c1, c2))
        {
            strategyUser.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, cell);
        }

        var zone = strategyUser.Tectonic.GetZone(c1);
        if (zone.Equals(strategyUser.Tectonic.GetZone(c2)))
        {
            foreach (var cell in zone)
            {
                if (cell == c1 || cell == c2) continue;
                strategyUser.ChangeBuffer.ProposePossibilityRemoval(one.Possibility, cell);
            }
        }

        return strategyUser.ChangeBuffer.Commit(new XChainsReportBuilder()) && OnCommitBehavior == OnCommitBehavior.Return;
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

public class XChainsReportBuilder : IChangeReportBuilder<IUpdatableTectonicSolvingState, ITectonicHighlighter>
{
    public ChangeReport<ITectonicHighlighter> Build(IReadOnlyList<SolverProgress> changes, IUpdatableTectonicSolvingState snapshot)
    {
        throw new NotImplementedException();
    }
}