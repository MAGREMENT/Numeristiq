using System.Collections.Generic;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil;

public static class ColorHelper //TODO use more + add parent history
{
    //TODO see if possible to do without ColoredElement
    public static List<ColoredVertices<T>> Color<T>(LinkGraph<ILinkGraphElement> graph) 
    {
        var result = new List<ColoredVertices<T>>();
        HashSet<ILinkGraphElement> visited = new();

        foreach (var start in graph)
        {
            if (visited.Contains(start)) continue;

            ColoredVertices<T> cv = new();
            cv.Add((T)start, Coloring.On);
            visited.Add(start);

            Queue<ColoredElement> queue = new();
            queue.Enqueue(new ColoredElement(start, Coloring.On));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
                {
                    if (visited.Contains(friend)) continue;

                    cv.Add((T)friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement(friend, opposite));
                }
                
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    cv.Add((T)friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement(friend, opposite));
                }
            }

            result.Add(cv);
        }

        return result;
    }
}

public class ColoredElement
{
    public ColoredElement(ILinkGraphElement element, Coloring coloring)
    {
        Element = element;
        Coloring = coloring;
    }

    public ILinkGraphElement Element { get; }
    public Coloring Coloring { get; }
}