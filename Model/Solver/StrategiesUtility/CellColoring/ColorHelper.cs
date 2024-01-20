using System.Collections.Generic;
using Model.Solver.StrategiesUtility.CellColoring.ColoringAlgorithms;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.StrategiesUtility.CellColoring;

public static class ColorHelper
{
    public static IColoringAlgorithm Algorithm { get; } = new QueueColoringAlgorithm();

    public static TR ColorFromStart<TB, TR>(Color<TB> colorMethod, ILinkGraph<TB> graph, TB start,
        Coloring firstColor = Coloring.On, bool history = false) where TB : IChainingElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
        if(history) result.ActivateHistoryTracking();
        var visited = new HashSet<TB>();
        
        result.NewStart();
        colorMethod(graph, result, visited, start, firstColor);
        
        return result;
    }
    
    public static TR ColorAll<TB, TR>(Color<TB> colorMethod, ILinkGraph<TB> graph, Coloring firstColor = Coloring.On,
        bool history = false) where TB : IChainingElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
        if(history) result.ActivateHistoryTracking();
        var visited = new HashSet<TB>();

        foreach (var start in graph)
        {
            if (visited.Contains(start)) continue;
            
            result.NewStart();
            colorMethod(graph, result, visited, start, firstColor); 
        }

        return result;
    }
}

public delegate void Color<T>(ILinkGraph<T> graph, IColoringResult<T> result,
    HashSet<T> visited, T start, Coloring firstColor) where T : IChainingElement;