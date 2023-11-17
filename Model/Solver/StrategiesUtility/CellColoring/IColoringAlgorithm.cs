using System.Collections.Generic;
using Model.Solver.StrategiesUtility.LinkGraph;

namespace Model.Solver.StrategiesUtility.CellColoring;

public interface IColoringAlgorithm
{
    void ColoringWithoutRules<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ILinkGraphElement;
    
    void ColorWithRules<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ILinkGraphElement;
    
    void ColorWithRulesAndLinksJump<T>(LinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ILinkGraphElement;
}