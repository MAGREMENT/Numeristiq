using System.Collections.Generic;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringAlgorithms;

public class QueueColoringAlgorithm : IColoringAlgorithm
{
    public TR ColoringWithoutRules<T, TR>(LinkGraph<T> graph) where T : ILinkGraphElement where TR : IColoringResult<T>, new()
    {
        var result = new TR();
        HashSet<T> visited = new();

        foreach (var start in graph)
        {
            if (visited.Contains(start)) continue;
            
            result.NewStart();
            result.AddColoredElement(start, Coloring.On);
            visited.Add(start);

            Queue<ColoredElement<T>> queue = new();
            queue.Enqueue(new ColoredElement<T>(start, Coloring.On));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
                {
                    if (visited.Contains(friend)) continue;

                    result.AddColoredElement(friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement<T>(friend, opposite));
                }
                
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    result.AddColoredElement(friend, opposite);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement<T>(friend, opposite));
                }
            }
        }

        return result;
    }

    public TR SimpleColoring<T, TR>(LinkGraph<T> graph) where T : ILinkGraphElement where TR : IColoringResult<T>, new()
    {
        return new TR();
    }

    public TR ComplexColoring<T, TR>(LinkGraph<T> graph) where T : ILinkGraphElement where TR : IColoringResult<T>, new()
    {
        return new TR();
    }
}

public class ColoredElement<T>
{
    public ColoredElement(T element, Coloring coloring)
    {
        Element = element;
        Coloring = coloring;
    }

    public T Element { get; }
    public Coloring Coloring { get; }
}