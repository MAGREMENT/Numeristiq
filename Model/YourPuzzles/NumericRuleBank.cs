using System;
using System.Collections.Generic;
using Model.Utility;
using Model.Utility.Collections;
using Model.Utility.Collections.Lexicography;
using Model.Utility.Collections.Lexicography.Nodes;
using Model.YourPuzzles.Rules;

namespace Model.YourPuzzles;

public static class NumericRuleBank
{
    private static readonly IGlobalNumericPuzzleRuleCrafter[] _global =
    {
        new NonRepeatingLinesNumericPuzzleRuleCrafter(),
        new DifferentNeighborsNumericPuzzleRuleCrafter()
    };

    private static readonly ILocalNumericPuzzleRuleCrafter[] _local =
    {
        new NonRepeatingBatchNumericPuzzleRuleCrafter(),
        new GreaterThanNumericPuzzleRuleCrafter()
    };

    private static readonly Trie<int> _trie = new ListTrie<int>();

    static NumericRuleBank()
    {
        for (int i = 0; i < _global.Length; i++)
        {
            if(!_trie.Add(_global[i].Abbreviation, i)) throw new ArgumentException("Already exists");
        }
        
        for (int i = 0; i < _local.Length; i++)
        {
            if(!_trie.Add(_local[i].Abbreviation, i + _global.Length)) throw new ArgumentException("Already exists");
        }
    }

    public static ILocalNumericPuzzleRule Craft(int index, IReadOnlyList<Cell> cells)
    {
        return _local[index].Craft(cells);
    }

    public static IGlobalNumericPuzzleRule Craft(int index)
    {
        return _global[index].Craft();
    }

    public static INumericPuzzleRule? Craft(string abbreviation, string data)
    {
        if (!_trie.TryGet(abbreviation, out var i)) return null;

        INumericPuzzleRuleCrafter crafter = i >= _global.Length ? _local[i - _global.Length] : _global[i];
        return crafter.Craft(data);
    }

    public static IEnumerable<RuleBankSearchResult> SearchFor(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells)
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

public interface INumericPuzzleRuleCrafter : INamed
{
    string Abbreviation { get; }
    INumericPuzzleRule? Craft(string s);
}

public interface IGlobalNumericPuzzleRuleCrafter : INumericPuzzleRuleCrafter
{
    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle);
    public IGlobalNumericPuzzleRule Craft();
}

public interface ILocalNumericPuzzleRuleCrafter : INumericPuzzleRuleCrafter
{
    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle, IReadOnlyList<Cell> cells);
    public ILocalNumericPuzzleRule Craft(IReadOnlyList<Cell> cells);
}