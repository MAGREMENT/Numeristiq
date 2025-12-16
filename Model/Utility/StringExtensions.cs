using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Utility;

public static class StringExtensions
{
    public static bool EqualsCaseInsensitive(this string s, string other)
    {
        if (s.Length != other.Length) return false;
        for (int i = 0; i < s.Length; i++)
        {
            if (char.ToLower(s[i]) != char.ToLower(other[i])) return false;
        }

        return true;
    }

    public static bool ContainsCaseInsensitive(this string s, string other)
    {
        for (int i = 0; i < s.Length - other.Length; i++)
        {
            if (char.ToLower(s[i]) != char.ToLower(other[0])) continue;

            bool ok = true;
            for (int j = 1; j < other.Length; j++)
            {
                if (char.ToLower(s[i + j]) != char.ToLower(other[j]))
                {
                    ok = false;
                    break;
                }
            }

            if (ok) return true;
        }

        return false;
    }

    public static string Repeat(this string s, int number)
    {
        if (number < 0) return "";

        var builder = new StringBuilder();
        for (int i = 0; i < number; i++)
        {
            builder.Append(s);
        }

        return builder.ToString();
    }

    public static string Repeat(this char s, int number)
    {
        if (number < 0) return "";

        var builder = new StringBuilder();
        for (int i = 0; i < number; i++)
        {
            builder.Append(s);
        }

        return builder.ToString();
    }

    public static string FillEvenlyWith(this string s, char fill, int desiredLength)
    {
        var toAdd = desiredLength - s.Length;
        var db2 = toAdd / 2;
        return fill.Repeat(db2 + toAdd % 2) + s + fill.Repeat(db2);
    }

    public static string FillRightWith(this string s, char fill, int desiredLength)
    {
        var toAdd = desiredLength - s.Length;
        return s + fill.Repeat(toAdd);
    }

    public static string FillLeftWith(this string s, char fill, int desiredLength)
    {
        var toAdd = desiredLength - s.Length;
        return fill.Repeat(toAdd) + s;
    }

    public static bool TryReadCell(this string s, out Cell cell) => s.TryReadCell(0, s.Length, out cell);

    public static bool TryReadCell(this string s, int from, int to, out Cell cell)
    {
        var row = -1;
        var buffer = 0;

        for (int i = from + 1; i < to; i++)
        {
            var c = s[i];
            if (char.IsDigit(c))
            {
                buffer *= 10;
                buffer += c - '0';
            }
            else
            {
                if (row != -1)
                {
                    cell = new Cell();
                    return false;
                }

                row = buffer;
                buffer = 0;
            }
        }

        if (row <= 0 || buffer <= 0)
        {
            cell = new Cell();
            return false;
        }

        cell = new Cell(row - 1, buffer - 1);
        return true;
    }

    public static List<Cell>? TryReadCells(this string s)
    {
        List<Cell> cells = new();

        int start = 0;
        int current = 1;
        for (; current < s.Length; current++)
        {
            if (s[current] == 'r')
            {
                if (!s.TryReadCell(start, current, out var cell)) return null;

                cells.Add(cell);
                start = current;
            }
        }

        if (!s.TryReadCell(start, current, out var last)) return null;

        cells.Add(last);
        return cells;
    }

    public static string ToHexString(this int n)
    {
        var builder = new StringBuilder();
        var alphabet = HexadecimalAlphabet.Instance;
        for (int offset = 28; offset >= 0; offset -= 4)
        {
            var v = (n >> offset) & 0xF;
            if (v == 0 && builder.Length == 0) continue;

            builder.Append(alphabet.ToChar(v));
        }

        return builder.ToString();
    }

    public static int FromHexString(this string s)
    {
        int result = 0;
        var alphabet = HexadecimalAlphabet.Instance;
        var max = Math.Min(7, s.Length - 1);
        for (int i = max; i >= 0; i--)
        {
            result |= alphabet.ToInt(s[i]) << ((max - i) * 4);
        }

        return result;
    }
    
    public static string ToGridReport<T>(this IReadOnlyList<T> list, int colCount, 
        ToGridCells<T> tgc, params ColumnTitle[][] titles)
    {
        var builder = new StringBuilder();
        list.ToGridReport(builder, colCount, tgc, titles);
        return builder.ToString();
    }

    public static void ToGridReport<T>(this IReadOnlyList<T> list, StringBuilder builder, int colCount, 
        ToGridCells<T> tgc, params ColumnTitle[][] titles)
    {
        Span<int> widthCap = stackalloc int[colCount];

        foreach (var title in titles)
        {
            int curr = 0;
            foreach (var t in title)
            {
                if (t.From < curr || t.To < curr) continue;

                var per = (t.Title.Length + 2) / (t.To - t.From);
                var rem = (t.Title.Length + 2) % (t.To - t.From);
                for (curr = t.From; curr < t.To; curr++)
                {
                    widthCap[curr] = Math.Max(widthCap[curr], per + (rem > 0 ? 1 : 0));
                    rem--;
                }

                curr = t.To;
            }
        }
        
        foreach (var el in list)
        {
            int i = 0;
            foreach (var s in tgc(el))
            {
                if (i >= colCount) break;

                widthCap[i] = Math.Max(widthCap[i], s.Length + 2);
                i++;
            }
        }

        foreach (var title in titles)
        {
            int curr = 0;

            foreach (var t in title)
            {
                if (t.From < curr || t.To < curr) continue;

                for (; curr < t.From && curr < colCount; curr++)
                {
                    builder.Append(' '.Repeat(widthCap[curr]));
                    if(curr < colCount - 1) builder.Append('|');
                }

                var size = 0;
                for (; curr < t.To && curr < colCount; curr++)
                {
                    if (size > 0) size++;
                    size += widthCap[curr];
                }

                builder.Append(t.Title.FillEvenlyWith(' ', size));
                if(curr < colCount) builder.Append('|');

                if (curr == colCount) break;
            }

            builder.Append('\n');
            ToGridLine(builder, widthCap);
        }
            
        foreach (var el in list)
        {
            int i = 0;
            foreach (var s in tgc(el))
            {
                if (i >= colCount) break;
                
                builder.Append(s.FillEvenlyWith(' ', widthCap[i]));
                if(i < colCount - 1) builder.Append('|');
                i++;
            }

            builder.Append('\n');
            ToGridLine(builder, widthCap);
        }
    }
    
    private static void ToGridLine(StringBuilder builder, Span<int> widthCap)
    {
        for (int i = 0; i < widthCap.Length; i++)
        {
            builder.Append('-'.Repeat(widthCap[i]));
            if (i < widthCap.Length - 1) builder.Append('+');
        }

        builder.Append('\n');
    }
}

public delegate IEnumerable<string> ToGridCells<in T>(T element);

public readonly struct ColumnTitle
{
    public readonly int From;
    public readonly int To;
    public readonly string Title;

    public ColumnTitle(string title, int from, int to)
    {
        Title = title;
        From = from;
        To = to;
    }

    public static ColumnTitle[] Single(string s) => [new(s, 0, int.MaxValue)];

    public static ColumnTitle[] Follow(params (string, int)[] f)
    {
        return Follow(0, f);
    }
    
    public static ColumnTitle[] Follow(int curr, params (string, int)[] f)
    {
        var result = new ColumnTitle[f.Length];

        for (int i = 0; i < f.Length; i++)
        {
            result[i] = new ColumnTitle(f[i].Item1, curr, curr + f[i].Item2);
            curr += f[i].Item2;
        }
        
        return result;
    }
    
    public static ColumnTitle[] Follow(params string[] f)
    {
        var result = new ColumnTitle[f.Length];

        for (int i = 0; i < f.Length; i++)
        {
            result[i] = new ColumnTitle(f[i], i, i + 1);
        }
        
        return result;
    }
}