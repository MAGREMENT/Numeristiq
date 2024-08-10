using System;
using System.Text;

namespace Model.Binairos;

public static class BinairoTranslator
{
    public static string TranslateLineFormat(IReadOnlyBinairo binairo)
    {
        if (binairo.RowCount == 0 || binairo.ColumnCount == 0) return string.Empty;
        
        var builder = new StringBuilder($"{binairo.RowCount}x{binairo.ColumnCount}:");
        for (int row = 0; row < binairo.RowCount; row++)
        {
            for (int col = 0; col < binairo.ColumnCount; col++)
            {
                var n = binairo[row, col];
                builder.Append(n == 0 ? "." : (n - 1).ToString());
            }
        }

        return builder.ToString();
    }

    public static Binairo TranslateLineFormat(string s)
    {
        Span<int> dimension = stackalloc int[2];
        dimension.Clear();

        int d = 0;
        int i = 0;
        for (; i < s.Length; i++)
        {
            var c = s[i];
            if (c is 'x' or ':')
            {
                d++;
                if(d > 1) break;
            }
            else
            {
                dimension[d] *= 10;
                dimension[d] += c - '0';
            }
        }

        var result = new Binairo(dimension[0], dimension[1]);
        if (result.RowCount == 0 || result.ColumnCount == 0) return result;
        
        int start = ++i;
        int n;
        for (; i < s.Length && (n = i - start) <= result.RowCount * result.ColumnCount; i++)
        {
            var c = s[i];
            if (c != '.' && c != ' ')
            {
                result[n / result.ColumnCount, n % result.ColumnCount] = c - '0' + 1;
            }
        }

        return result;
    }
}