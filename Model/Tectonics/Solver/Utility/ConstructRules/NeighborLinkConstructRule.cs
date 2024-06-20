using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Tectonics.Solver.Utility.ConstructRules;

public class NeighborLinkConstructRule : IConstructRule<ITectonicSolverData, ITectonicElement>
{
    public void Apply(ILinkGraph<ITectonicElement> linkGraph, ITectonicSolverData solverData)
    {
        Dictionary<IZone, List<Cell>> zoneBuffer = new();
        
        for (int row = 0; row < solverData.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < solverData.Tectonic.ColumnCount; col++)
            {
                var poss = solverData.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                foreach (var p in poss.EnumeratePossibilities())
                {
                    foreach (var cell in TectonicCellUtility.GetNeighbors(row, col,
                                 solverData.Tectonic.RowCount, solverData.Tectonic.ColumnCount))
                    {
                        if (solverData.PossibilitiesAt(cell).Contains(p))
                        {
                            linkGraph.Add(new CellPossibility(row, col, p), new CellPossibility(cell, p), LinkStrength.Weak);
                            var zone = solverData.Tectonic.GetZone(cell);
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
                        
                        var pos = solverData.ZonePositionsFor(entry.Key, p);
                        if (pos.Count == entry.Value.Count + 1)
                        {
                            Cell? buffer = null;
                            foreach (var n in pos.EnumeratePositions(entry.Key))
                            {
                                var cell = entry.Key[n];
                                if (!TectonicCellUtility.AreNeighbors(entry.Key[n], new Cell(row, col)))
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

    public void Apply(ILinkGraph<CellPossibility> linkGraph, ITectonicSolverData solverData)
    {
        for (int row = 0; row < solverData.Tectonic.RowCount; row++)
        {
            for (int col = 0; col < solverData.Tectonic.ColumnCount; col++)
            {
                var poss = solverData.PossibilitiesAt(row, col);
                if (poss.Count == 0) continue;

                foreach (var p in poss.EnumeratePossibilities())
                {
                    foreach (var cell in TectonicCellUtility.GetNeighbors(row, col,
                                 solverData.Tectonic.RowCount, solverData.Tectonic.ColumnCount))
                    {
                        if (solverData.PossibilitiesAt(cell).Contains(p)) linkGraph.Add(
                            new CellPossibility(row, col, p), new CellPossibility(cell, p), LinkStrength.Weak);
                    }
                }
            }
        }
    }
}