using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class RandomLayoutBackTrackingFilledTectonicGenerator : IFilledPuzzleGenerator<ITectonic>
{
    private readonly TectonicBackTracker _backTracker = new()
    {
        StopAt = 1
    };
    
    public GridSizeRandomizer Randomizer { get; } = new(2, 10);

    public ITectonic Generate()
    {
        var size = Randomizer.GenerateSize();
        ITectonic result = new ArrayTectonic(size.RowCount, size.ColumnCount);

        while(true)
        {
            if (result.Zones.Count > 0) result.ClearZones();

            for (int r = 0; r < result.RowCount; r++)
            {
                for (int c = 0; c < result.ColumnCount; c++)
                {
                    var current = new Cell(r, c);
                    var zone = result.GetZone(current);
                    if (zone.Count == IZone.MaxCount) continue;

                    if(r < result.RowCount - 1 && Randomizer.GenerateChance(1, 2))
                        result.MergeZones(current, new Cell(r + 1, c));
                    
                    if (c < result.ColumnCount - 1 && Randomizer.GenerateChance(1, 2))
                        result.MergeZones(current, new Cell(r, c + 1));
                    
                    zone = result.GetZone(current);
                    if (zone.Count > 1) continue;

                    if (r > 0)
                    {
                        var cell = new Cell(r - 1, c);
                        if (result.GetZone(cell).Count == 1) result.MergeZones(current, cell);
                    }

                    if (c > 0)
                    {
                        var cell = new Cell(r, c - 1);
                        if (result.GetZone(cell).Count == 1) result.MergeZones(current, cell);
                    }
                }
            }

            _backTracker.Set(result, new TectonicPossibilitiesGiver(result));
            if (_backTracker.Fill()) break;
        }

        return result;
    }
    
    public ITectonic Generate(out List<Cell> removableCells)
    {
        var t = Generate();
        removableCells = GetRemovableCells(t);
        return t;
    }

    private static List<Cell> GetRemovableCells(ITectonic tectonic)
    {
        var list = new List<Cell>(tectonic.RowCount * tectonic.ColumnCount);
        for (int row = 0; row < tectonic.RowCount; row++)
        {
            for (int c = 0; c < tectonic.ColumnCount; c++)
            {
                list.Add(new Cell(row, c));
            }
        }

        return list;
    }
}