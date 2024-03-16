using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudoku.Solver;
using Model.Sudoku.Solver.StrategiesUtility;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;
using Model.Utility;

namespace Model.Tectonic.Strategies;

public class NeighboringZonesStrategy : TectonicStrategy //TODO correct this
{
    public NeighboringZonesStrategy() : base("Neighboring Zones", StrategyDifficulty.Hard, OnCommitBehavior.ChooseBest)
    {
    }

    public override void Apply(IStrategyUser strategyUser)
    {
        for (int z = 0; z < strategyUser.Tectonic.Zones.Count; z++)
        {
            var zone = strategyUser.Tectonic.Zones[z];
            for (int n = 1; n <= zone.Count; n++)
            {
                var pos = strategyUser.ZonePositionsFor(z, n);
                if (pos.Count == 2)
                {
                    var asArray = pos.ToArray();
                    if (Search(strategyUser, n, zone[asArray[0]], zone[asArray[1]])) return;
                    if (Search(strategyUser, n, zone[asArray[1]], zone[asArray[0]])) return;
                }
            }
        }
    }

    private bool Search(IStrategyUser strategyUser, int possibility, Cell startOff, Cell startOn)
    {
        HashSet<Cell> on = new();
        Dictionary<Cell, Cell> off = new();
        Queue<Cell> queue = new();
        List<IZone> zoneBuffer = new();
        List<Cell> cellBuffer = new();
        
        on.Add(startOn);
        queue.Enqueue(startOn);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in Cells.GetNeighbors(current.Row, current.Column,
                         strategyUser.Tectonic.RowCount, strategyUser.Tectonic.ColumnCount))
            {
                if (!strategyUser.PossibilitiesAt(neighbor).Contains(possibility)) continue;

                if (!off.TryAdd(neighbor, current) || neighbor == startOff) continue;
                
                var zone = strategyUser.Tectonic.GetZone(neighbor);
                if (!zoneBuffer.Contains(zone)) zoneBuffer.Add(zone);
            }

            foreach (var zone in zoneBuffer)
            {
                foreach (var cell in zone)
                {
                    if (strategyUser.PossibilitiesAt(cell).Contains(possibility) && !off.ContainsKey(cell)) cellBuffer.Add(cell);
                }

                if (cellBuffer.Count == 1 && on.Add(cellBuffer[0]))
                {
                    queue.Enqueue(cellBuffer[0]);

                    foreach (var cell in Cells.SharedSeenCells(strategyUser.Tectonic, startOff, cellBuffer[0]))
                    {
                        strategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                    }

                    if (strategyUser.ChangeBuffer.NotEmpty() && strategyUser.ChangeBuffer
                            .Commit(new NeighboringZonesReportBuilder(possibility, on, off, startOff)) 
                                                             && OnCommitBehavior == OnCommitBehavior.Return) return true;
                }
                
                cellBuffer.Clear();
            }
            
            zoneBuffer.Clear();
        }

        return false;
    }
}

public class NeighboringZonesReportBuilder : IChangeReportBuilder<ITectonicSolvingState, ITectonicHighlighter>
{
    private readonly int _possibility;
    private readonly Cell _startOff;
    private readonly HashSet<Cell> _on;
    private readonly Dictionary<Cell, Cell> _off;

    public NeighboringZonesReportBuilder(int possibility, IEnumerable<Cell> on, IDictionary<Cell, Cell> off, Cell startOff)
    {
        _possibility = possibility;
        _startOff = startOff;
        _on = new HashSet<Cell>(on);
        _off = new Dictionary<Cell, Cell>(off);
    }

    public ChangeReport<ITectonicHighlighter> Build(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        return new ChangeReport<ITectonicHighlighter>("", lighter =>
        {
            foreach (var cell in _on)
            {
                lighter.HighlightPossibility(cell, _possibility, ChangeColoration.CauseOnOne);
            }

            foreach (var entry in _off)
            {
                lighter.HighlightPossibility(entry.Key, _possibility, ChangeColoration.CauseOffTwo);
                lighter.CreateLink(new CellPossibility(entry.Key, _possibility),
                    new CellPossibility(entry.Value, _possibility), LinkStrength.Weak);
            }
            
            lighter.HighlightPossibility(_startOff, _possibility, ChangeColoration.CauseOffOne);

            ChangeReportHelper.HighlightChanges(lighter, changes);
        });
    }
}