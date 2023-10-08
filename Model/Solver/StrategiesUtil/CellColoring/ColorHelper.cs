using System.Collections.Generic;
using Model.Solver.StrategiesUtil.CellColoring.ColoringAlgorithms;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring;

public static class ColorHelper //TODO use more + add parent history
{
    public static IColoringAlgorithm Algorithm { get; } = new QueueColoringAlgorithm();

    public static TR ColorFromStart<TB, TR>(Color<TB> colorMethod, LinkGraph<TB> graph, TB start, Coloring firstColor = Coloring.On)
        where TB : ILinkGraphElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
        var visited = new HashSet<TB>();
        
        result.NewStart();
        colorMethod(graph, result, visited, start, firstColor);
        
        return result;
    }
    
    public static TR ColorAll<TB, TR>(Color<TB> colorMethod, LinkGraph<TB> graph, Coloring firstColor = Coloring.On)
        where TB : ILinkGraphElement where TR : IColoringResult<TB>, new()
    {
        var result = new TR();
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

public delegate void Color<T>(LinkGraph<T> graph, IColoringResult<T> result,
    HashSet<T> visited, T start, Coloring firstColor) where T : ILinkGraphElement;