using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

namespace Model.Binairos;

public class FilledBinairoPuzzleGenerator : IFilledPuzzleGenerator<Binairo>
{
    private readonly BinairoBackTracker _backTracker = new();
    
    public GridSizeRandomizer Randomizer { get; } = new(2, 20);
    
    public Binairo Generate()
    {
        var size = Randomizer.GenerateSize();
        _backTracker.Set(new Binairo(size.RowCount, size.ColumnCount));
        _backTracker.Fill();
        return _backTracker.Current;
    }

    public Binairo Generate(out List<Cell> removableCells)
    {
        var b = Generate();
        removableCells = GetRemovableCells(b);
        return b;
    }

    private static List<Cell> GetRemovableCells(IReadOnlyBinairo binairo)
    {
        var result = new List<Cell>();
        for (int r = 0; r < binairo.RowCount; r++)
        {
            for (int c = 0; c < binairo.ColumnCount; c++)
            {
                result.Add(new Cell(r, c));
            }
        }

        return result;
    }
}