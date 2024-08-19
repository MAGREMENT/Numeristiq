using System.Collections.Generic;
using Model.Core.Graphs.Coloring.ColoringAlgorithms;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Core.Graphs.Coloring;

public static class ColorHelper
{
    public static IColoringAlgorithm Algorithm { get; } = new QueueColoringAlgorithm();

    public static TR ColorFromStart<TB, TR>(Color<TB> colorMethod, IGraph<TB, LinkStrength> graph, TB start,
        ElementColor firstColor = ElementColor.On, bool history = false) where TB : ISudokuElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
        if(history) result.ActivateHistoryTracking();
        var visited = new HashSet<TB>();
        
        result.NewStart();
        colorMethod(graph, result, visited, start, firstColor);
        
        return result;
    }
    
    public static TR ColorFromStart<TB, TR>(ConditionalColor<TB> colorMethod, IConditionalGraph<TB, LinkStrength, ElementColor> graph, TB start,
        ElementColor firstColor = ElementColor.On, bool history = false) where TB : ISudokuElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
        if(history) result.ActivateHistoryTracking();
        var visited = new HashSet<TB>();
        
        result.NewStart();
        colorMethod(graph, result, visited, start, firstColor);
        
        return result;
    }
    
    public static TR ColorAll<TB, TR>(Color<TB> colorMethod, IGraph<TB, LinkStrength> graph, ElementColor firstColor = ElementColor.On,
        bool history = false) where TB : ISudokuElement where TR : IColoringResult<TB>, new()
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

public delegate void Color<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result,
    HashSet<T> visited, T start, ElementColor firstColor) where T : notnull;
    
public delegate void ConditionalColor<T>(IConditionalGraph<T, LinkStrength, ElementColor> graph, IColoringResult<T> result,
    HashSet<T> visited, T start, ElementColor firstColor) where T : notnull;