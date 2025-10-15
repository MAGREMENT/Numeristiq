using System;
using System.Collections.Generic;
using Model.Core.Settings;
using Model.Core.Syntax;
using Model.Utility.BitSets;
using Model.Utility.Collections;

namespace Model.YourPuzzles.Rules;

public class NonRepeatingLinesNumericPuzzleRule : IGlobalNumericPuzzleRule
{
    public const string OfficialName = "Non-Repeating Lines";
    public const string OfficialAbbreviation = "nl";
    
    public string Name => OfficialName;
    public string Abbreviation => OfficialAbbreviation;
    
    public IEnumerable<ISetting> EnumerateSettings()
    {
        yield break;
    }

    public bool IsRespected(IReadOnlyNumericYourPuzzle board)
    {
        for (int row = 0; row < board.RowCount; row++)
        {
            var bitSet = new ReadOnlyBitSet16();
            for (int col = 0; col < board.ColumnCount; col++)
            {
                var n = board[row, col];
                if (n <= 0 || bitSet.Contains(n)) return false;

                bitSet += n;
            }
        }
        
        for (int col = 0; col < board.RowCount; col++)
        {
            var bitSet = new ReadOnlyBitSet16();
            for (int row = 0; row < board.ColumnCount; row++)
            {
                var n = board[row, col];
                if (n <= 0 || bitSet.Contains(n)) return false;

                bitSet += n;
            }
        }

        return true;
    }

    public string DataToString()
    {
        return string.Empty;
    }
    
    public ISyntaxElement ToSyntax()
    {
        return null!;
    }
}

public class NonRepeatingLinesNumericPuzzleRuleCrafter : IGlobalNumericPuzzleRuleCrafter
{
    public string Name => NonRepeatingLinesNumericPuzzleRule.OfficialName;
    public string Abbreviation => NonRepeatingLinesNumericPuzzleRule.OfficialAbbreviation;
    
    public INumericPuzzleRule Craft(string s)
    {
        return new NonRepeatingLinesNumericPuzzleRule();
    }

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle)
    {
        return puzzle is { RowCount: <= 9, ColumnCount: <= 9 } && puzzle.AreAllEnabled()
            && !puzzle.GlobalRules.Has<NonRepeatingLinesNumericPuzzleRule>();
    }

    public IGlobalNumericPuzzleRule Craft()
    {
        return new NonRepeatingLinesNumericPuzzleRule();
    }
}