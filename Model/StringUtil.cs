using System.Text;

namespace Model;

public static class StringUtil
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

    public static string FillWithSpace(string s, int desiredLength)
    {
        return s + Repeat(" ", desiredLength - s.Length);
    }
}