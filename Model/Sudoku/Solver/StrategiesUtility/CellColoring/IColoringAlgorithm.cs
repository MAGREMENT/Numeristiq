using System.Collections.Generic;
using Model.Sudoku.Solver.StrategiesUtility.Graphs;

namespace Model.Sudoku.Solver.StrategiesUtility.CellColoring;

public interface IColoringAlgorithm
{
    void ColorWithoutRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
    
    void ColorWithRules<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
    
    void ColorWithRulesAndLinksJump<T>(ILinkGraph<T> graph, IColoringResult<T> result, HashSet<T> visited, T start,
        Coloring firstColor = Coloring.On) where T : ISudokuElement;
}