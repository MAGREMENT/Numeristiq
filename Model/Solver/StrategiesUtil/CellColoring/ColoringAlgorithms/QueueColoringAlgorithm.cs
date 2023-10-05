using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring.ColoringAlgorithms;

public class QueueColoringAlgorithm : IColoringAlgorithm
{
    public R SimpleColoring<T, R>(LinkGraph<T> graph) where T : notnull where R : IColoringResult<T>, new()
    {
        return new R();
    }

    public R ComplexColoring<T, R>(LinkGraph<T> graph) where T : notnull where R : IColoringResult<T>, new()
    {
        return new R();
    }
}