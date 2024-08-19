using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Utility.Coloring.ColoringAlgorithms;

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

    public void ColorWithRulesAndLinksJump<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, 
        HashSet<T> visited, T start, ElementColor firstColor = ElementColor.On) where T : ISudokuElement
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
            else if (current.Element is CellPossibility pos) //TODO replace with conditional graph ?
            {
                T? row = default;
                bool rowB = true;
                T? col = default;
                bool colB = true;
                T? mini = default;
                bool miniB = true;
                T? cell = default;
                bool cellB = true;
                
            
                foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Weak))
                {
                    if (friend is not CellPossibility friendPos) continue;
                    if (friendPos.Possibility == pos.Possibility)
                    {
                        if (rowB && friendPos.Row == pos.Row)
                        {
                            if (result.TryGetColoredElement(friend, out var coloring))
                            {
                                if (coloring == ElementColor.On) rowB = false;
                            }
                            else
                            {
                                if (row is null) row = friend;
                                else rowB = false;  
                            }
                    
                        }

                        if (colB && friendPos.Column == pos.Column)
                        {
                            if (result.TryGetColoredElement(friend, out var coloring))
                            {
                                if (coloring == ElementColor.On) colB = false;
                            }
                            else
                            {
                                if (col is null) col = friend;
                                else colB = false;
                            }
                        }

                        if (miniB && friendPos.Row / 3 == pos.Row / 3 && friendPos.Column / 3 == pos.Column / 3)
                        {
                            if (result.TryGetColoredElement(friend, out var coloring))
                            {
                                if (coloring == ElementColor.On) miniB = false;
                            }
                            else
                            {
                                if (mini is null) mini = friend;
                                else miniB = false;
                            }
                        }
                    }
                    else
                    {
                        if (cellB && friendPos.Row == pos.Row && friendPos.Column == pos.Column)
                        {
                            if (result.TryGetColoredElement(friend, out var coloring))
                            {
                                if (coloring == ElementColor.On) cellB = false;
                            }
                            else
                            {
                                if (cell is null) cell = friend;
                                else cellB = false;  
                            }
                    
                        }
                    }
                }

                if (row is not null && rowB)
                {
                    result.AddColoredElement(row, ElementColor.On, current.Element);
                    visited.Add(row);
                    queue.Enqueue(new ColoredElement<T>(row, ElementColor.On));
                }

                if (col is not null && colB)
                {
                    result.AddColoredElement(col, ElementColor.On, current.Element);
                    visited.Add(col);
                    queue.Enqueue(new ColoredElement<T>(col, ElementColor.On));
                }

                if (mini is not null && miniB)
                {
                    result.AddColoredElement(mini, ElementColor.On, current.Element);
                    visited.Add(mini);
                    queue.Enqueue(new ColoredElement<T>(mini, ElementColor.On));
                }

                if (cell is not null && cellB)
                {
                    result.AddColoredElement(cell, ElementColor.On, current.Element);
                    visited.Add(cell);
                    queue.Enqueue(new ColoredElement<T>(cell, ElementColor.On));
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