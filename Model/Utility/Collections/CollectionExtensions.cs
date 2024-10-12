using System;
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
            if (e is TNew) return true;
        }

        return false;
    }

    public static int GetSequenceHashCode<T>(this IEnumerable<T> enumerable) where T : notnull
    {
        var hash = 0;
        foreach (var e in enumerable)
        {
            HashCode.Combine(e.GetHashCode(), hash);
        }

        return hash;
    }

    public static void CopyInto<T>(this IReadOnlyList<T> list, int ind1, T[] output, int ind2, int len)
    {
        for (int i = 0; i < len; i++)
        {
            output[ind2 + i] = list[ind1 + i];
        }
    }
}

public delegate string Stringify<in T>(T value);