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
}