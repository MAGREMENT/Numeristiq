using System.Collections.Generic;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Core.Graphs.Coloring;

public interface IColoringAlgorithm
{
    void ColorWithoutRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        ElementColor firstColor = ElementColor.On) where T : notnull;

    void ColorWithRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        ElementColor firstColor = ElementColor.On) where T : notnull;

    void ColorConditionalWithRules<T>(IConditionalGraph<T, LinkStrength, ElementColor> graph, IColoringResult<T> result,
        HashSet<T> visited, T start, ElementColor firstColor = ElementColor.On) where T : notnull
    {
        graph.Values = result;
        ColorWithRules(graph, result, visited, start, firstColor);
    }
}