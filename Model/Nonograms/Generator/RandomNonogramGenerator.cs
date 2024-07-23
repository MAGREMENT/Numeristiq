using System.Collections.Generic;
using Model.Core.BackTracking;
using Model.Core.Generators;
using Model.Nonograms.Solver;
using Model.Utility;
using Model.Utility.BitSets;

namespace Model.Nonograms.Generator;

public class RandomNonogramGenerator : IPuzzleGenerator<Nonogram>
{
    private readonly NaiveNonogramBackTracker _backTracker = new(new Nonogram(), ConstantAvailabilityChecker.Instance)
    {
        StopAt = 2
    };
    
    public GridSizeRandomizer Randomizer { get; } = new(3, 20);

    public event OnNextStep? StepDone;
    public bool KeepSymmetry { get; set; }
    public bool KeepUniqueness { get; set; } = true;

    public Nonogram Generate()
    {
        Nonogram result;

        do
        {
            var size = Randomizer.GenerateSize();
            var buffer = new CalibratedInfiniteBitmap(size.RowCount, size.ColumnCount);

            if (KeepSymmetry)
            {
                var isNotPair = size.RowCount % 2 == 1;
                var rMax = size.RowCount / 2 + (isNotPair ? 1 : 0);
                var cMax = 0;
                if (isNotPair) cMax = size.ColumnCount / 2 + (size.ColumnCount % 2 == 1 ? 1 : 0);
                
                for (int r = 0; r < rMax; r++)
                {
                    for (int c = 0; c < size.ColumnCount; c++)
                    {
                        if (isNotPair && r == rMax - 1 && c >= cMax) break;
                        
                        if (Randomizer.GenerateChance(1, 2))
                        {
                            buffer.Add(r, c);
                            buffer.Add(size.RowCount - 1 - r, size.ColumnCount - 1 - c);
                        }
                    }
                }
            }
            else
            {
                for (int r = 0; r < size.RowCount; r++)
                {
                    for (int c = 0; c < size.ColumnCount; c++)
                    {
                        if (Randomizer.GenerateChance(1, 2)) buffer.Add(r, c);
                    }
                }
            }

            for (int r = 0; r < size.RowCount; r++)
            {
                if (buffer.IsRowEmpty(r)) buffer.Add(r, Randomizer.GenerateBetween(0, size.ColumnCount));
            }

            for (int c = 0; c < size.ColumnCount; c++)
            {
                if (buffer.IsColumnEmpty(c)) buffer.Add(Randomizer.GenerateBetween(0, size.RowCount), c);
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

            result = new Nonogram();
            result.Add(h, v);

            if (!KeepUniqueness) break;

            _backTracker.Set(result);
        } while (_backTracker.Count() > 1);
        
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