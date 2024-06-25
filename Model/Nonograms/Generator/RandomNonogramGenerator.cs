using System.Collections.Generic;
using Model.Utility;

namespace Model.Nonograms.Generator;

public class RandomNonogramGenerator
{
    public GridSizeRandomizer Randomizer { get; } = new(3, 20);

    public Nonogram Generate()
    {
        var size = Randomizer.GenerateSize();
        var array = new bool[size.RowCount, size.ColumnCount];

        for (int r = 0; r < size.RowCount; r++)
        {
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (Randomizer.GenerateChance(1, 2)) array[r, c] = true;
            }
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
}