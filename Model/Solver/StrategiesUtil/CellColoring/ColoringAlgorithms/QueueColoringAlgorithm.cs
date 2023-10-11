using System.Collections.Generic;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringAlgorithms;

public class QueueColoringAlgorithm : IColoringAlgorithm
{
    public void ColoringWithoutRules<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On)
        where T : ILinkGraphElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);

        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

            foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }
                
            foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }
        }
    }

    public void SimpleColoring<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On) where T : ILinkGraphElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);
        
        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

            foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }

            if (opposite == Coloring.Off)
            {
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    result.AddColoredElement(friend, opposite, current.Element);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement<T>(friend, opposite));
                }
            }
        }
    }

    public void ComplexColoring<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On)
        where T : ILinkGraphElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);
        
        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

            foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }

            if (opposite == Coloring.Off)
            {
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (visited.Contains(friend)) continue;

                    result.AddColoredElement(friend, opposite, current.Element);
                    visited.Add(friend);
                    queue.Enqueue(new ColoredElement<T>(friend, opposite));
                }
            }
            else if (current.Element is CellPossibility pos)
            {
                T? row = default;
                bool rowB = true;
                T? col = default;
                bool colB = true;
                T? mini = default;
                bool miniB = true;
            
                foreach (var friend in graph.GetLinks(current.Element, LinkStrength.Weak))
                {
                    if (friend is not CellPossibility friendPos) continue;
                    if (rowB && friendPos.Row == pos.Row)
                    {
                        if (result.TryGetColoredElement(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) rowB = false;
                        }
                        else
                        {
                            if (row is null) row = friend;
                            else rowB = false;  
                        }
                    
                    }

                    if (colB && friendPos.Col == pos.Col)
                    {
                        if (result.TryGetColoredElement(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) colB = false;
                        }
                        else
                        {
                            if (col is null) col = friend;
                            else colB = false;
                        }
                    }

                    if (miniB && friendPos.Row / 3 == pos.Row / 3 && friendPos.Col / 3 == pos.Col / 3)
                    {
                        if (result.TryGetColoredElement(friend, out var coloring))
                        {
                            if (coloring == Coloring.On) miniB = false;
                        }
                        else
                        {
                            if (mini is null) mini = friend;
                            else miniB = false;
                        }
                    }
                }

                if (row is not null && rowB)
                {
                    result.AddColoredElement(row, Coloring.On, current.Element);
                    visited.Add(row);
                    queue.Enqueue(new ColoredElement<T>(row, Coloring.On));
                }

                if (col is not null && colB)
                {
                    result.AddColoredElement(col, Coloring.On, current.Element);
                    visited.Add(col);
                    queue.Enqueue(new ColoredElement<T>(col, Coloring.On));
                }

                if (mini is not null && miniB)
                {
                    result.AddColoredElement(mini, Coloring.On, current.Element);
                    visited.Add(mini);
                    queue.Enqueue(new ColoredElement<T>(mini, Coloring.On));
                }
            }
        }
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