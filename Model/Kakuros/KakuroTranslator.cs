using System;
using System.Text;
using Model.Utility;

namespace Model.Kakuros;

/// <summary>
/// Sum format : (rr,cc>aa,ll;)*:(rr,cc,n;)*
/// rr : row
/// cc : col
/// > : orientation (> or v)
/// aa : amount
/// ll : length
/// n : number
/// </summary>
public static class KakuroTranslator
{
    public static string TranslateSumFormat(IReadOnlyKakuro kakuro)
    {
        var builder = new StringBuilder();
        foreach (var sum in kakuro.Sums)
        {
            var start = sum.GetStartCell();
            var orientation = sum.Orientation == Orientation.Vertical ? 'v' : '>';
            builder.Append($"{start.Row},{start.Column}{orientation}{sum.Amount},{sum.Length};");
        }

        bool done = false;
        foreach (var cell in kakuro.EnumerateCells())
        {
            var n = kakuro[cell.Row, cell.Column];
            if (n == 0) continue;

            if (!done)
            {
                builder.Append(':');
                done = true;
            }

            builder.Append($"{cell.Row},{cell.Column},{n};");
        }

        return builder.ToString();
    }
    
    public static IKakuro TranslateSumFormat(string s)
    {
        var index = s.IndexOf(':');
        var sums = index == -1 ? s : s[..index];
        Span<int> valuesArray = stackalloc int[3];
        valuesArray.Clear();

        try
        {
            var result = new SumListKakuro();
            foreach (var sum in sums.Split(';'))
            {
                if (sum.Length == 0) continue;
                
                var buffer = 0;
                var cursor = 0;
                var orientation = Orientation.Horizontal;
                foreach (var c in sum)
                {
                    if (char.IsDigit(c))
                    {
                        buffer *= 10;
                        buffer += c - '0';
                    }
                    else
                    {
                        orientation = c switch
                        {
                            'v' => Orientation.Vertical,
                            '>' => Orientation.Horizontal,
                            _ => orientation
                        };

                        valuesArray[cursor++] = buffer;
                        buffer = 0;
                    }
                }

                IKakuroSum toAdd = orientation == Orientation.Horizontal
                    ? new HorizontalKakuroSum(new Cell(valuesArray[0], valuesArray[1]),
                        valuesArray[2], buffer)
                    : new VerticalKakuroSum(new Cell(valuesArray[0], valuesArray[1]),
                        valuesArray[2], buffer);

                result.ForceSum(toAdd);
            }
            
            if(index == -1) return result;
            
            foreach (var p in s[(index + 1)..].Split(';'))
            {
                var buffer = 0;
                var cursor = 0;
                foreach (var c in p)
                {
                    if (char.IsDigit(c))
                    {
                        buffer *= 10;
                        buffer += c - '0';
                    }
                    else
                    {
                        valuesArray[cursor++] = buffer;
                        buffer = 0;
                    }
                }

                result[valuesArray[0], valuesArray[1]] = buffer;
            }

            return result;
        }
        catch (Exception)
        {
            return new SumListKakuro();
        }
    }
}