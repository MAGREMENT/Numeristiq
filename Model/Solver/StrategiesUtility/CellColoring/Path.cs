using System;
using System.Collections.Generic;
using System.Text;
using Global.Enums;
using Model.Solver.Helpers.Highlighting;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.CellColoring;

public class Path<T> where T : ILinkGraphElement
{
    public Path(T[] elements, LinkStrength[] links)
    {
        if (elements.Length != links.Length + 1) throw new ArgumentException();
        Elements = elements;
        Links = links;
    }

    public Path(T element)
    {
        Elements = new[] { element };
        Links = Array.Empty<LinkStrength>();
    }

    public T[] Elements { get; }
    public LinkStrength[] Links { get; }

    public int Count => Elements.Length;

    public void Highlight(IHighlightable highlighter)
    {
        for (int i = 0; i < Links.Length; i++)
        {
            var current = Links[i];
            highlighter.HighlightLinkGraphElement(Elements[i + 1], current == LinkStrength.Strong
                ? ChangeColoration.CauseOnOne : ChangeColoration.CauseOffOne);
            highlighter.CreateLink(Elements[i], Elements[i + 1], current);
        }
        
        if(Elements.Length > 0 && Links.Length > 0) highlighter.HighlightLinkGraphElement(Elements[0], Links[0] == LinkStrength.Strong
            ? ChangeColoration.CauseOffOne : ChangeColoration.CauseOnOne);
    }

    public Loop<T>? TryMakeLoop(Path<T> path)
    {
        if (!path.Elements[0].Equals(Elements[0]) || !path.Elements[^1].Equals(Elements[^1])) return null;
        HashSet<T> present = new HashSet<T>(Elements);

        var total = Count + path.Count - 2;
        
        var elements = new T[total];
        var links = new LinkStrength[total];

        Array.Copy(Elements, 0, elements, 0, Elements.Length);
        var cursor = Elements.Length;
        for (int i = path.Elements.Length - 2; i > 0; i--)
        {
            var current = path.Elements[i];
            if (present.Contains(current)) return null;

            elements[cursor++] = current;
        }

        Array.Copy(Links, 0, links, 0, Links.Length);
        cursor = Links.Length;
        for (int i = path.Links.Length - 1; i >= 0; i--)
        {
            links[cursor++] = path.Links[i];
        }

        return new Loop<T>(elements, links);
    }
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Elements.Length - 1; i++)
        {
            builder.Append(Elements[i] + (Links[i] == LinkStrength.Strong ? " = " : " - "));
        }

        builder.Append(Elements[^1]);
        return builder.ToString();
    }
}