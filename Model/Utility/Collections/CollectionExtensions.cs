using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Model.Utility.Collections;

public static class CollectionExtensions
{
    public static string ToStringSequence<T>(this IReadOnlyList<T> list, string separator) where T : notnull
    {
        if (list.Count == 0) return string.Empty;

        var builder = new StringBuilder(list[0].ToString());
        for (int i = 1; i < list.Count; i++)
        {
            builder.Append(separator + list[i]);
        }

        return builder.ToString();
    }
    
    public static string ToStringSequence<T>(this IReadOnlyList<T> list, string separator,
        Stringify<T> stringify) where T : notnull
    {
        if (list.Count == 0) return string.Empty;

        var builder = new StringBuilder(stringify(list[0]));
        for (int i = 1; i < list.Count; i++)
        {
            builder.Append(separator + stringify(list[i]));
        }

        return builder.ToString();
    }
    
    public static string ToStringSequence<T>(this IEnumerable<T> enumerable, string separator) where T : notnull
    {
        bool firstDone = false;
        var builder = new StringBuilder();
        foreach(var element in enumerable)
        {
            if (firstDone) builder.Append(separator);
            else firstDone = true;

            builder.Append(element);
        }

        return builder.ToString();
    }
    
    public static string ToStringSequence<T>(this IEnumerable<T> enumerable, string separator,
        Stringify<T> stringify) where T : notnull
    {
        bool firstDone = false;
        var builder = new StringBuilder();
        foreach(var element in enumerable)
        {
            if (firstDone) builder.Append(separator);
            else firstDone = true;

            builder.Append(stringify(element));
        }

        return builder.ToString();
    }

    public static IEnumerable<TNew> ForAll<TNew>(this IEnumerable enumerable)
    {
        foreach (var e in enumerable)
        {
            if (e is TNew n) yield return n;
        }
    }
    
    public static bool Has<TNew>(this IEnumerable enumerable)
    {
        foreach (var e in enumerable)
        {
            if (e is TNew n) return true;
        }

        return false;
    }
}

public delegate string Stringify<in T>(T value);