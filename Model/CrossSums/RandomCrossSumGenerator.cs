using Model.Core.Generators;
using Model.Utility;

namespace Model.CrossSums;

public class RandomCrossSumGenerator : IPuzzleGenerator<CrossSum>
{
    public event OnNextStep? StepDone;
    public bool KeepSymmetry { get; set; } //TODO
    public bool KeepUniqueness { get; set; }

    public GridSizeRandomizer Randomizer { get; } = new(5, 10);

    private readonly CrossSumBackTracker _backTracker = new();

    public RandomCrossSumGenerator()
    {
        _backTracker.StopAt = 2;
    }
    
    public CrossSum Generate()
    {
        var size = Randomizer.GenerateSize();
        var result = new CrossSum(size.RowCount, size.ColumnCount);

        for (int i = 0; i < size.RowCount; i++)
        {
            for (int j = 0; j < size.ColumnCount; j++)
            {
                result[i, j] = Randomizer.GenerateBetween(1, 10);
            }
        }
        
        StepDone?.Invoke(StepType.FilledGenerated);
        
        for (int r = 0; r < size.RowCount; r++)
        {
            for (int c = 0; c < size.ColumnCount; c++)
            {
                if(!Randomizer.GenerateChance(1, 2)) continue;

                var v = result[r, c];
                result.AddToExpectedForRow(r, v);
                result.AddToExpectedForRow(r, c);
                
                if(!KeepUniqueness) continue;
                
                _backTracker.Set(result);
                var count = _backTracker.Count();
                if (count != 1)
                {
                    result.AddToExpectedForRow(r, -v);
                    result.AddToExpectedForColumn(c, -v);
                }
            }
        }
        
        StepDone?.Invoke(StepType.PuzzleGenerated);
        return result;
    }

    public CrossSum[] Generate(int count)
    {
        var result = new CrossSum[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = Generate();
        }

        return result;
    }
}