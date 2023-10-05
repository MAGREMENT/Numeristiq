using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring;

public interface IColoringAlgorithm
{
    R SimpleColoring<T, R>(LinkGraph<T> graph) where R : IColoringResult<T>, new() where T : notnull;
    R ComplexColoring<T, R>(LinkGraph<T> graph) where R : IColoringResult<T>, new() where T : notnull;
}