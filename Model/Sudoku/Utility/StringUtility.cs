using System.Text;

namespace Model.Sudoku.Utility;

public static class StringUtility
{
    public static string Repeat(string s, int number)
    {
        if (number < 0) return "";
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < number; i++)
        {
            builder.Append(s);
        }

        return builder.ToString();
    }
    
    public static string Repeat(char s, int number)
    {
        if (number < 0) return "";
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < number; i++)
        {
            builder.Append(s);
        }

        return builder.ToString();
    }

    public static string FillEvenlyWith(string s, char fill, int desiredLength)
    {
        var toAdd = desiredLength - s.Length;
        var db2 = toAdd / 2;
        return Repeat(fill, db2 + toAdd % 2) + s + Repeat(fill, db2);
    }
    
    public static string FillWith(string s, char fill, int desiredLength)
    {
        var toAdd = desiredLength - s.Length;
        return s + Repeat(fill, toAdd);
    }
}