using System.Collections.Generic;
using Model.Core.Generators;
using Model.Utility;

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
        var array = new bool[size.RowCount, size.ColumnCount]; //TODO to infinite bitmap

        for (int r = 0; r < size.RowCount; r++)
        {
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (Randomizer.GenerateChance(1, 2)) array[r, c] = true;
            }
        }

        for (int r = 0; r < size.RowCount; r++)
        {
            var notOk = true;
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (!array[r, c]) continue;
                
                notOk = false;
                break;
            }
            
            if(notOk) array[r, Randomizer.GenerateBetween(0, size.ColumnCount)] = true;
        }
        
        for (int c = 0; c < size.ColumnCount; c++)
        {
            var notOk = true;
            for (int r = 0; r < size.ColumnCount; r++)
            {
                if (!array[r, c]) continue;
                
                notOk = false;
                break;
            }
            
            if(notOk) array[Randomizer.GenerateBetween(0, size.RowCount), c] = true;
        }

        List<int[]> h = new();
        List<int[]> v = new();
        List<int> current = new();

        for (int r = 0; r < size.RowCount; r++)
        {
            int streak = 0;
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (array[r, c]) streak++;
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
                if (array[r, c]) streak++;
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