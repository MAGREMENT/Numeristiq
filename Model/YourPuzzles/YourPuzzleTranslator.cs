using System;
using System.Text;

namespace Model.YourPuzzles;

/// <summary>
/// Line format :
/// RxC:(N|.|-)+:(RU)*
/// where RxC = rows + "x" + columns
/// where (RC)* = from 0 to infinity cells (disabled cells)
/// where (N|.)+ = cells numbers or . for void or - for disabled from top left
/// where (RU;)* = from 0 to infinity rules
/// </summary>
public static class YourPuzzleTranslator
{
    public static string TranslateLineFormat(NumericYourPuzzle puzzle)
    {
        var builder = new StringBuilder($"{puzzle.RowCount}x{puzzle.ColumnCount}");

        builder.Append(':');
        for (int row = 0; row < puzzle.RowCount; row++)
        {
            for (int col = 0; col < puzzle.ColumnCount; col++)
            {
                if (!puzzle.IsEnabled(row, col)) builder.Append('-');
                else
                {
                    var n = puzzle[row, col];
                    builder.Append(n == 0 ? '.' : n + '0');
                }
            }
        }

        builder.Append(':');
        foreach (var rule in puzzle.GlobalRules)
        {
            builder.Append(rule.Abbreviation + rule.DataToString() + ';');
        }
        
        foreach (var rule in puzzle.LocalRules)
        {
            builder.Append(rule.Abbreviation + rule.DataToString() + ';');
        }
        
        return builder.ToString();
    }

    public static NumericYourPuzzle TranslateLineFormat(string s)
    {
        var split = s.Split();
        if (split.Length != 3) return new NumericYourPuzzle(0, 0);

        var current = split[0];
        var index = current.IndexOf('x');
        if (index == -1) return new NumericYourPuzzle(0, 0);
        
        if(!int.TryParse(current[..index], out var r)) return new NumericYourPuzzle(0, 0);
        if(!int.TryParse(current[(index + 1)..], out var c)) return new NumericYourPuzzle(0, 0);

        var puzzle = new NumericYourPuzzle(r, c);

        foreach (var rString in split[2].Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            if(rString.Length <= 2) continue;

            var rule = NumericRuleBank.Craft(rString[..2], rString[2..]);
            if(rule is not null) puzzle.AddRuleUnchecked(rule);
        }
        
        current = split[1];
        int i = 0;
        for (r = 0; r < puzzle.RowCount && i < current.Length; r++)
        {
            for (c = 0; c < puzzle.ColumnCount && i < current.Length; c++)
            {
                var ch = s[i];
                switch (ch)
                {
                    case '-' :
                        puzzle.DisableCell(r, c);
                        break;
                    case '0' :
                    case '.' : break;
                    default :
                        puzzle[r, c] = ch - '0';
                        break;
                }

                i++;
            }
        }

        return puzzle;
    }
}