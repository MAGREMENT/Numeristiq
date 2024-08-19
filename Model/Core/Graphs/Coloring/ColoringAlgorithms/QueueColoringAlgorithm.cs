using System.Collections.Generic;

namespace Model.Core.Graphs.Coloring.ColoringAlgorithms;

public class QueueColoringAlgorithm : IColoringAlgorithm
{
    public void ColorWithoutRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited,
        T start, ElementColor firstColor = ElementColor.On) where T : notnull
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);

        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == ElementColor.Off ? ElementColor.On : ElementColor.Off;

            foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }
                
            foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Weak))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }
        }
    }

    public void ColorWithRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, 
        T start, ElementColor firstColor = ElementColor.On) where T : notnull
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);

        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == ElementColor.Off ? ElementColor.On : ElementColor.Off;

            foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }

            if (opposite == ElementColor.Off)
            {
                foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    result.AddColoredElement(friend, opposite, current.Element);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement<T>(friend, opposite));
                }
            }
        }
    }
}

public class ColoredElement<T>
{
    public ColoredElement(T element, ElementColor coloring)
    {
        Element = element;
        Coloring = coloring;
    }

    public T Element { get; }
    public ElementColor Coloring { get; }
}