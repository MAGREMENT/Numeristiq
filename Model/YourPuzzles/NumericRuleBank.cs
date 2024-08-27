using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.Collections;
using Model.YourPuzzles.Rules;

namespace Model.YourPuzzles;

public class NumericRuleBank
{
    private readonly IGlobalNumericPuzzleRuleCrafter[] _global = Array.Empty<IGlobalNumericPuzzleRuleCrafter>();

    private readonly ILocalNumericPuzzleRuleCrafter[] _local =
    {
        new UniqueBatchNumericPuzzleRuleCrafter(),
        new GreaterThanNumericPuzzleRuleCrafter()
    };

    public ILocalNumericPuzzleRule Craft(int index, IReadOnlyList<Cell> cells)
    {
        return _local[index].Craft(cells);
    }

    public IGlobalNumericPuzzleRule Craft(int index)
    {
        return _global[index].Craft();
    }

    public IEnumerable<RuleBankSearchResult> SearchFor(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
    {
        for (int i = 0; i < _global.Length; i++)
        {
            var r = _global[i];
            if (r.CanCraft(puzzle)) yield return new RuleBankSearchResult(r.Name, true, true, i);
        }
        
        for (int i = 0; i < _local.Length; i++)
        {
            var r = _local[i];
            yield return new RuleBankSearchResult(r.Name, false, r.CanCraft(puzzle, cells), i);
        }
    }
}

public record RuleBankSearchResult(string Name, bool IsGlobal, bool CanBeCrafted, int Index);

public interface IGlobalNumericPuzzleRuleCrafter : INamed
{
    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle);
    public IGlobalNumericPuzzleRule Craft();
}

public interface ILocalNumericPuzzleRuleCrafter : INamed
{
    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells);
    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells);
}