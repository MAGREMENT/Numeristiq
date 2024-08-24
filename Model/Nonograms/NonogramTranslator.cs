using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Nonograms;

public static class NonogramTranslator
{
    public static Nonogram TranslateLineFormat(string s)
    {
        try
        {
            var result = new Nonogram();

            List<IEnumerable<int>> hValues = new();
            List<IEnumerable<int>> vValues = new();
            var currentCollection = vValues;
            var currentValues = new List<int>();
            int buffer = 0;
            bool cells = false;
            int i = 0;
            
            for(; i < s.Length; i++)
            {
                if (cells) break;
                var c = s[i];
                switch (c)
                {
                    case ' ' : break;
                    case '.' :
                        currentValues.Add(buffer);
                        buffer = 0;
                        break;
                    case '-' :
                        currentValues.Add(buffer);
                        currentCollection.Add(currentValues.ToArray());
                        buffer = 0;
                        currentValues.Clear();
                        break;
                    case ':' :
                        currentValues.Add(buffer);
                        currentCollection.Add(currentValues.ToArray());
                        buffer = 0;
                        currentValues.Clear();
                        
                        i++;
                        if (hValues == currentCollection) cells = true;
                        else currentCollection = hValues;
                        break;
                    default:
                        buffer *= 10;
                        buffer += c - '0';
                        break;
                }
            }

            if (buffer != 0)
            {
                currentValues.Add(buffer);
                currentCollection.Add(currentValues.ToArray());
            }
            result.Add(hValues, vValues);

            if (cells)
            {
                foreach (var cell in s[i..].Split('-', StringSplitOptions.RemoveEmptyEntries))
                {
                    var index = cell.IndexOf('.');
                    if(index == -1) continue;

                    result[int.Parse(cell[..index]), int.Parse(cell[(index + 1)..])] = true;
                }
            }
            return result;
        }
        catch (Exception)
        {
            return new Nonogram();
        }
    }
    
    public static string TranslateLineFormat(IReadOnlyNonogram nonogram)
    {
        var builder = new StringBuilder();

        for (int i = 0; i < 2; i++)
        {
            var data = i == 0 
                ? nonogram.VerticalLines 
                : nonogram.HorizontalLines;

            for (int j = 0; j < data.Count; j++)
            {
                bool notFirst = false;
                foreach (var v in data[j])
                {
                    if (notFirst) builder.Append('.');
                    else notFirst = true;
                    builder.Append(v);
                }
                
                if (j != data.Count - 1) builder.Append('-');
                else if(i == 0) builder.Append("::");
            }
        }

        var atLeastOne = false;
        for (int row = 0; row < nonogram.RowCount; row++)
        {
            for (int col = 0; col < nonogram.ColumnCount; col++)
            {
                if (!nonogram[row, col]) continue;

                if (atLeastOne) builder.Append($"-{row}.{col}");
                else
                {
                    builder.Append($"::{row}.{col}");
                    atLeastOne = true;
                }
            }
        }
        
        return builder.ToString();
    }
}