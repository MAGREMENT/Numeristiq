using Model.Solver.StrategiesUtil;
using Model.Solver.StrategiesUtil.CellColoring;
using Model.Solver.StrategiesUtil.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtil.LinkGraph;

namespace Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV4 : IAlternatingChainAlgorithm<ILinkGraphElement> //TODO fix -> Take into account mono-directionnal links
{
    public void Run(IStrategyManager view, LinkGraph<ILinkGraphElement> graph, IAlternatingChainType<ILinkGraphElement> chainType)
    {
        foreach (var start in graph)
        {
            if (start is not CellPossibility) continue;
            var onColoring = ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.On, true);
            var offColoring = ColorHelper.ColorFromStart<ILinkGraphElement, ColoringDictionary<ILinkGraphElement>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.Off, true);

            foreach (var entry in offColoring)
            {
                if (onColoring.TryGetColoredElement(entry.Key, out var coloring))
                {
                    var path1 = onColoring.History!.GetPathToRoot(entry.Key, coloring);
                    var path2 = offColoring.History!.GetPathToRoot(entry.Key, entry.Value);
                    if(path2.Count < 2 || path1.Count < 2 || path1.Count + path2.Count < 6) continue;
                    
                    var loop = path1.TryMakeLoop(path2);
                    if (loop is null) continue;

                    switch (entry.Value, coloring)
                    {
                        case (Coloring.On, Coloring.Off) :
                        case (Coloring.Off, Coloring.On) :
                            chainType.ProcessFullLoop(view, loop);
                            break;
                        case(Coloring.On, Coloring.On) :
                            chainType.ProcessStrongInference(view, entry.Key, loop);
                            break;
                        case (Coloring.Off, Coloring.Off) :
                            chainType.ProcessWeakInference(view, entry.Key, loop);
                            break;
                    }
                }
            }
        }
    }
}