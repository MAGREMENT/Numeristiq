using System;
using System.Collections.Generic;
using System.Text;
using Model.Core.Graphs;

namespace Model.Sudokus.Solver.Utility.Graphs;

public static class SudokuChainExtensions
{
    public static Loop<T, LinkStrength>? TryMakeLoop<T>(this Chain<T, LinkStrength> path1, bool isMono1,
        Chain<T, LinkStrength> path2, bool isMono2) where T : ISudokuElement
    {
        if (isMono1 && isMono2) return null;
        
        if (!path2.Elements[0].Equals(path1.Elements[0]) || !path2.Elements[^1].Equals(path1.Elements[^1])) return null;
        
        var total = path1.Count + path2.Count - 2;
        
        var all = new HashSet<T>(path1.Elements);
        all.UnionWith(path2.Elements);
        if (all.Count != total) return null;

        var elements = new T[total];
        var links = new LinkStrength[total];

        Chain<T, LinkStrength> first, second;
        switch (isMono1, isMono2)
        {
            case(false, false) :
            case(true, false) :
                first = path1;
                second = path2;
                break;
            case (false, true) :
                first = path2;
                second = path1;
                break;
            default : return null;
        }

        Array.Copy(first.Elements, 0, elements, 0, first.Elements.Length - 1);
        Array.Copy(second.Elements, 1, elements, first.Elements.Length - 1, second.Elements.Length - 1);
        Array.Reverse(elements, first.Elements.Length - 1, second.Elements.Length - 1);
            
        Array.Copy(first.Links, 0, links, 0, first.Links.Length);
        Array.Copy(second.Links, 0, links, first.Links.Length, second.Links.Length);
        Array.Reverse(links, first.Links.Length, second.Links.Length);

        return new Loop<T, LinkStrength>(elements, links);
    }
    
    public static int MaxRank<TElement, TLink>(this Chain<TElement, TLink> chain) where TLink : notnull
        where TElement : ISudokuElement
    {
        var result = 0;
        foreach (var element in chain.Elements)
        {
            result = Math.Max(result, element.DifficultyRank);
        }

        return result;
    }

    public static string ToLinkChainString<TElement>(this Chain<TElement, LinkStrength> chain) where TElement : notnull
    {
        var builder = new StringBuilder();
        for (int i = 0; i < chain.Elements.Length - 1; i++)
        {
            builder.Append(chain.Elements[i] + (chain.Links[i] == LinkStrength.Strong ? " = " : " - "));
        }

        builder.Append(chain.Elements[^1]);
        return builder.ToString();
    }
    
    public static string ToLinkLoopString<TElement>(this Loop<TElement, LinkStrength> loop) where TElement : notnull
    {
        string result = loop.Elements[0] + (loop.Links[0] == LinkStrength.Strong ? " = " : " - ");
        for (int i = 1; i < loop.Elements.Length; i++)
        {
            if(i == loop.Elements.Length - 1) result += loop.Elements[i] + (loop.LastLink == LinkStrength.Strong ? " = " : " - ");
            else result += loop.Elements[i] + (loop.Links[i] == LinkStrength.Strong ? " = " : " - ");
        }

        return result;
    }
}