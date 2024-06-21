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
                        currentCollection.Add(currentValues.ToArray());
                        buffer = 0;
                        currentValues.Clear();
                        break;
                    case ':' :
                        currentValues.Add(buffer);
                        currentCollection.Add(currentValues.ToArray());
                        buffer = 0;
                        currentValues.Clear();
                        currentCollection = hValues;
                        i++;
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
                ? nonogram.VerticalLineCollection 
                : nonogram.HorizontalLineCollection;

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
        
        return builder.ToString();
    }
}