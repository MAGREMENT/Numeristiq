using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Generator;

public class RandomNonogramGenerator : IPuzzleGenerator<Nonogram>
{
    public GridSizeRandomizer Randomizer { get; } = new(3, 20);

    public event OnNextStep? StepDone;
    public bool KeepSymmetry { get; set; }
    public bool KeepUniqueness { get; set; }

    public Nonogram Generate()
    {
        var size = Randomizer.GenerateSize();
        var buffer = new CalibratedInfiniteBitmap(size.RowCount, size.ColumnCount);

        for (int r = 0; r < size.RowCount; r++)
        {
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (Randomizer.GenerateChance(1, 2)) buffer.Add(r, c);
            }
        }

        for (int r = 0; r < size.RowCount; r++)
        {
            if(buffer.IsRowEmpty(r)) buffer.Add(r, Randomizer.GenerateBetween(0, size.ColumnCount));
        }
        
        for (int c = 0; c < size.ColumnCount; c++)
        {
            if(buffer.IsColumnEmpty(c)) buffer.Add(Randomizer.GenerateBetween(0, size.RowCount), c);
        }

        List<int[]> h = new();
        List<int[]> v = new();
        List<int> current = new();

        for (int r = 0; r < size.RowCount; r++)
        {
            int streak = 0;
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (buffer.Contains(r, c)) streak++;
                else if (streak > 0)
                {
                    current.Add(streak);
                    streak = 0;
                }
            }

            if (streak > 0) current.Add(streak);
            h.Add(current.ToArray());
            current.Clear();
        }
        
        for (int c = 0; c < size.ColumnCount; c++)
        {
            int streak = 0;
            for (int r = 0; r < size.RowCount; r++)
            {
                if (buffer.Contains(r, c)) streak++;
                else if (streak > 0)
                {
                    current.Add(streak);
                    streak = 0;
                }
            }

            if (streak > 0) current.Add(streak);
            v.Add(current.ToArray());
            current.Clear();
        }

        var result = new Nonogram();
        result.Add(h, v);
        return result;
    }
    
    public Nonogram[] Generate(int count)
    {
        var result = new Nonogram[count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Generate();
        }

        return result;
    }
}