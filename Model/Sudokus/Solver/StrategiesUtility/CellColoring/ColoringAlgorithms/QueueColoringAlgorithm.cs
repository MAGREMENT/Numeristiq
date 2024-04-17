using System.Collections.Generic;
using Model.Sudokus.Solver.StrategiesUtility.Graphs;

namespace Model.Sudokus.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;

public class QueueColoringAlgorithm : IColoringAlgorithm
{
    public void ColorWithoutRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On)
        where T : ISudokuElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);

        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

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

    public void ColorWithRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On) where T : ISudokuElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);
        
        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

            foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }

            if (opposite == Coloring.Off)
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

    public void ColorWithRulesAndLinksJump<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start, Coloring firstColor = Coloring.On)
        where T : ISudokuElement
    {
        result.AddColoredElement(start, firstColor);
        visited.Add(start);
        
        Queue<ColoredElement<T>> queue = new();
        queue.Enqueue(new ColoredElement<T>(start, firstColor));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var opposite = current.Coloring == Coloring.Off ? Coloring.On : Coloring.Off;

            foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Strong))
            {
                if (visited.Contains(friend)) continue;

                result.AddColoredElement(friend, opposite, current.Element);
                visited.Add(friend);
                queue.Enqueue(new ColoredElement<T>(friend, opposite));
            }

            if (opposite == Coloring.Off)
            {
                foreach (var friend in graph.Neighbors(current.Element, LinkStrength.Weak))
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
                                if (coloring == Coloring.On) rowB = false;
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
                                if (coloring == Coloring.On) colB = false;
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
                                if (coloring == Coloring.On) miniB = false;
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
                                if (coloring == Coloring.On) cellB = false;
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

                if (cell is not null && cellB)
                {
                    result.AddColoredElement(cell, Coloring.On, current.Element);
                    visited.Add(cell);
                    queue.Enqueue(new ColoredElement<T>(cell, Coloring.On));
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