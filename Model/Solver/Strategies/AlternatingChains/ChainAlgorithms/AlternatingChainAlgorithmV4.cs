using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV4<T> : IAlternatingChainAlgorithm<T> where T : ILinkGraphElement
{
    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        foreach (var start in graph)
        {
            if (start is not CellPossibility) continue;
            var onColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.On, true);
            var offColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
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
                            if (chainType.ProcessFullLoop(view, loop) &&
                                chainType.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                        case(Coloring.On, Coloring.On) :
                            if (chainType.ProcessStrongInference(view, entry.Key, loop) &&
                                chainType.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                        case (Coloring.Off, Coloring.Off) :
                            if (chainType.ProcessWeakInference(view, entry.Key, loop) &&
                                chainType.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                    }
                }
            }
        }
    }
}