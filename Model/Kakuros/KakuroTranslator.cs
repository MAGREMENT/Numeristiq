using System;
using Model.Utility;

namespace Model.Kakuros;

/// <summary>
/// Sum format : (rr,cc>aa,ll;)*:(rr,cc,n;)*
/// rr : row
/// cc : col
/// > : direction (> or v)
/// aa : amount
/// ll : length
/// n : number
/// </summary>
public static class KakuroTranslator
{
    public static IKakuro TranslateSumFormat(string s)
    {
        var index = s.IndexOf(':');
        var sums = index == -1 ? s : s[..index];
        Span<int> valuesArray = stackalloc int[3];
        valuesArray.Clear();

        try
        {
            var result = new ArrayKakuro();
            foreach (var sum in sums.Split(';'))
            {
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

                result.AddSum(toAdd);
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
            return new ArrayKakuro();
        }
    }
}