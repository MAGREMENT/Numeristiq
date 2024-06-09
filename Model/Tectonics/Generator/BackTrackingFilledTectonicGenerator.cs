using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Tectonics.Generator;

public class BackTrackingFilledTectonicGenerator : IFilledPuzzleGenerator<ITectonic>
{
    private readonly IEmptyTectonicGenerator _emptyGenerator = new RandomEmptyTectonicGenerator();

    public ITectonic Generate()
    {
        var empty = _emptyGenerator.Generate();
        return BackTracking.Fill(empty, new TectonicPossibilitiesGiver(empty), 1)[0];
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