using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Utility.CellColoring;

public interface IColoringAlgorithm
{
    void ColorWithoutRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : notnull;

    void ColorWithRules<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : notnull;

    void ColorWithRulesAndLinksJump<T>(IGraph<T, LinkStrength> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
}