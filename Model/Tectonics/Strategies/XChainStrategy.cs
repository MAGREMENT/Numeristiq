using System.Collections.Generic;
using Model.Helpers;
using Model.Helpers.Changes;
using Model.Helpers.Highlighting;
using Model.Sudokus.Solver;
using Model.Sudokus.Solver.Utility;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Tectonics.Strategies;

public class XChainStrategy : TectonicStrategy
{
    public XChainStrategy() : base("X-Chains", StrategyDifficulty.Hard, InstanceHandling.BestOnly)
    {
    }

    public override void Apply(ITectonicStrategyUser strategyUser)
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

    private bool Search(ITectonicStrategyUser tectonicStrategyUser, int possibility, Cell startOff, Cell startOn)
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

            foreach (var neighbor in TectonicCellUtility.GetNeighbors(current.Row, current.Column,
                         tectonicStrategyUser.Tectonic.RowCount, tectonicStrategyUser.Tectonic.ColumnCount))
            {
                if (!tectonicStrategyUser.PossibilitiesAt(neighbor).Contains(possibility) 
                    || neighbor == startOff || !off.TryAdd(neighbor, current)) continue;
                
                var zone = tectonicStrategyUser.Tectonic.GetZone(neighbor);
                if (!zoneBuffer.Contains(zone)) zoneBuffer.Add(zone);
            }

            foreach (var zone in zoneBuffer)
            {
                foreach (var cell in zone)
                {
                    if (tectonicStrategyUser.PossibilitiesAt(cell).Contains(possibility) && !off.ContainsKey(cell)) cellBuffer.Add(cell);
                }

                if (cellBuffer.Count == 1 && on.Add(cellBuffer[0]))
                {
                    queue.Enqueue(cellBuffer[0]);

                    foreach (var cell in TectonicCellUtility.SharedSeenCells(tectonicStrategyUser.Tectonic, startOff, cellBuffer[0]))
                    {
                        tectonicStrategyUser.ChangeBuffer.ProposePossibilityRemoval(possibility, cell);
                    }

                    if (tectonicStrategyUser.ChangeBuffer.NotEmpty() && tectonicStrategyUser.ChangeBuffer
                            .Commit(new NeighboringZonesReportBuilder(possibility, on, off, startOff)) 
                                                             && StopOnFirstPush) return true;
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

    public ChangeReport<ITectonicHighlighter> BuildReport(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
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
    
    public Clue<ITectonicHighlighter> BuildClue(IReadOnlyList<SolverProgress> changes, ITectonicSolvingState snapshot)
    {
        return Clue<ITectonicHighlighter>.Default();
    }
}