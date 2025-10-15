using System.Collections.Generic;
using Model.Core.Settings;
using Model.Core.Syntax;
using Model.Tectonics;
using Model.Utility.Collections;

namespace Model.YourPuzzles.Rules;

public class DifferentNeighborsNumericPuzzleRule : IGlobalNumericPuzzleRule
{
    public const string OfficialName = "Different Neighbors";
    public const string OfficialAbbreviation = "dn";

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
            for (int col = 0; col < board.ColumnCount; col++)
            {
                if(!board.IsEnabled(row, col)) continue;
                
                var n = board[row, col];
                if(n == 0) continue;

                foreach (var neighbor in TectonicUtility.GetNeighbors(row, col, board.RowCount, board.ColumnCount))
                {
                    if(!board.IsEnabled(neighbor)) continue;

                    if (board[neighbor.Row, neighbor.Column] == n) return false;
                }
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

public class DifferentNeighborsNumericPuzzleRuleCrafter : IGlobalNumericPuzzleRuleCrafter
{
    public string Name => DifferentNeighborsNumericPuzzleRule.OfficialName;
    public string Abbreviation => DifferentNeighborsNumericPuzzleRule.OfficialAbbreviation;
    
    public INumericPuzzleRule? Craft(string s)
    {
        return new DifferentNeighborsNumericPuzzleRule();
    }

    public bool CanCraft(IReadOnlyNumericYourPuzzle puzzle)
    {
        return true;
    }

    public IGlobalNumericPuzzleRule Craft()
    {
        return new DifferentNeighborsNumericPuzzleRule();
    }
}