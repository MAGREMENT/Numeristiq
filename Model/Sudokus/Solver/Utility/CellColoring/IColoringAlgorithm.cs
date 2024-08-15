using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Utility.CellColoring;

public interface IColoringAlgorithm
{
    void ColorWithoutRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
    
    void ColorWithRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
    
    void ColorWithRulesAndLinksJump<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
}