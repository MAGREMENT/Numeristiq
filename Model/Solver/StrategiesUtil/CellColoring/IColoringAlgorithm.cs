using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.StrategiesUtil.CellColoring;

public interface IColoringAlgorithm
{
    TR ColoringWithoutRules<TB, TR>(LinkGraph<TB> graph) where TR : IColoringResult<TB>, new() where TB : ILinkGraphElement;
    TR SimpleColoring<TB, TR>(LinkGraph<TB> graph) where TR : IColoringResult<TB>, new() where TB : ILinkGraphElement;
    TR ComplexColoring<TB, TR>(LinkGraph<TB> graph) where TR : IColoringResult<TB>, new() where TB : ILinkGraphElement;
}