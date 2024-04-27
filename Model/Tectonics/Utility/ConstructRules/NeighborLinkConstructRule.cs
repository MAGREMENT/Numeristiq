using System.Collections.Generic;
using Model.Helpers.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Tectonics.Utility.ConstructRules;

public class NeighborLinkConstructRule : IConstructRule<ITectonicStrategyUser, ITectonicElement> //TODO fix for 5.4:0d0rd0r100r00d0rd0rd40d0r00r5d0r0r0
{
    public void Apply(ILinkGraph<ITectonicElement> linkGraph, ITectonicStrategyUser strategyUser)
    {
        Dictionary<IZone, List<Cell>> zoneBuffer = new();
        
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                foreach (var p in poss.EnumeratePossibilities())
                {
                    foreach (var cell in TectonicCellUtility.GetNeighbors(row, col,
                                 strategyUser.Tectonic.RowCount, strategyUser.Tectonic.ColumnCount))
                    {
                        if (strategyUser.PossibilitiesAt(cell).Contains(p))
                        {
                            linkGraph.Add(new CellPossibility(row, col, p), new CellPossibility(cell, p), LinkStrength.Weak);
                            var zone = strategyUser.Tectonic.GetZone(cell);
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
                        
                        var pos = strategyUser.ZonePositionsFor(entry.Key, p);
                        if (pos.Count == entry.Value.Count + 1)
                        {
                            Cell? buffer = null;
                            foreach (var cell in entry.Key)
                            {
                                if (!TectonicCellUtility.AreNeighbors(cell, new Cell(row, col)))
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

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ITectonicStrategyUser strategyUser)
    {
        for (int row = 0; row < strategyUser.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < strategyUser.Tectonic.ColumnCount; col++)
            {
                var poss = strategyUser.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                foreach (var p in poss.EnumeratePossibilities())
                {
                    foreach (var cell in TectonicCellUtility.GetNeighbors(row, col,
                                 strategyUser.Tectonic.RowCount, strategyUser.Tectonic.ColumnCount))
                    {
                        if (strategyUser.PossibilitiesAt(cell).Contains(p)) linkGraph.Add(
                            new CellPossibility(row, col, p), new CellPossibility(cell, p), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}