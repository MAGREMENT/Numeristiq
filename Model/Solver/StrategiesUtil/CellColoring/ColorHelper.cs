using Model.Solver.StrategiesUtil.CellColoring.ColoringAlgorithms;

namespace Model.Solver.StrategiesUtil.CellColoring;

public static class ColorHelper //TODO use more + add parent history
{
    private static readonly IColoringAlgorithm Algo = new QueueColoringAlgorithm();

    public static IColoringAlgorithm GetAlgorithm() => Algo;
}