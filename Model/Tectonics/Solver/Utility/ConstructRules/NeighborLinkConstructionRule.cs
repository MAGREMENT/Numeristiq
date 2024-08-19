using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Utility.ConstructRules;

public class NeighborLinkConstructionRule : IConstructionRule<ITectonicSolverData, IGraph<ITectonicElement, LinkStrength>>
{
    public static NeighborLinkConstructionRule Instance { get; } = new();
    
    private NeighborLinkConstructionRule() {}
    
    public int ID { get; } = UniqueConstructionRuleID.Next();
    
    public void Apply(IGraph<ITectonicElement, LinkStrength> linkGraph, ITectonicSolverData data)
    {
        Dictionary<IZone, List<Cell>> zoneBuffer = new();
        
        for (int row = 0; row < data.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < data.Tectonic.ColumnCount; col++)
            {
                var poss = data.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                foreach (var p in poss.EnumeratePossibilities())
                {
                    foreach (var cell in TectonicUtility.GetNeighbors(row, col,
                                 data.Tectonic.RowCount, data.Tectonic.ColumnCount))
                    {
                        if (data.PossibilitiesAt(cell).Contains(p))
                        {
                            linkGraph.Add(new CellPossibility(row, col, p), new CellPossibility(cell, p), LinkStrength.Weak);
                            var zone = data.Tectonic.GetZone(cell);
                            if (!zoneBuffer.TryGetValue(zone, out var list))
                            {
                                list = new List<Cell>();
                                zoneBuffer[zone] = list;
                            }

                            list.Add(cell);
                        }
                    }

                    foreach (var entry in zoneBuffer)
                    {
                        if (entry.Value.Count <= 1) continue;
                        
                        var pos = data.ZonePositionsFor(entry.Key, p);
                        if (pos.Count == entry.Value.Count + 1)
                        {
                            Cell? buffer = null;
                            foreach (var n in pos.EnumeratePositions(entry.Key))
                            {
                                var cell = entry.Key[n];
                                if (!TectonicUtility.AreNeighbors(entry.Key[n], new Cell(row, col)))
                                {
                                    buffer = cell;
                                }
                            }

                            if (buffer is null) continue;

                            var zoneGroup = new ZoneGroup(entry.Value, p);
                            linkGraph.Add(new CellPossibility(row, col, p), 
                                zoneGroup, LinkStrength.Weak, LinkType.MonoDirectional);
                            linkGraph.Add(zoneGroup, new CellPossibility(buffer.Value, p),
                                LinkStrength.Strong, LinkType.MonoDirectional);
                        }
                    }
                    
                    zoneBuffer.Clear();
                }
            }
        }
    }
}