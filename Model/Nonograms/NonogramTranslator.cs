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
            var currentCollection = hValues;
            var currentValues = new List<int>();
            int buffer = 0;
            
            for(int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                switch (c)
                {
                    case '.' :
                        currentValues.Add(buffer);
                        buffer = 0;
                        break;
                    case '-' :
                        currentValues.Add(buffer);
                        buffer = 0;
                        currentCollection.Add(currentValues.ToArray());
                        currentValues.Clear();
                        break;
                    case ':' :
                        currentCollection = vValues;
                        i++;
                        break;
                    default:
                        buffer *= 10;
                        buffer += c - '0';
                        break;
                }
            }

            result.Add(hValues, vValues);
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
                ? nonogram.HorizontalLineCollection 
                : nonogram.VerticalLineCollection;

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
                else if(i == 1) builder.Append("::");
            }
        }
        
        return builder.ToString();
    }
}