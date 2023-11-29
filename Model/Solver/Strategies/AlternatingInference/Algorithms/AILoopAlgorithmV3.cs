using Model.Solver.StrategiesUtility;
using Model.Solver.StrategiesUtility.CellColoring;
using Model.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV3<T> : IAlternatingInferenceAlgorithm<T> where T : ILinkGraphElement
{
    public AlgorithmType Type => AlgorithmType.Loop;
    
    public void Run(IStrategyManager strategyManager, LinkGraph<T> graph, IAlternatingInferenceType<T> type)
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
                            if (type.ProcessFullLoop(strategyManager, loop) &&
                                type.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                        case(Coloring.On, Coloring.On) :
                            if (type.ProcessStrongInferenceLoop(strategyManager, entry.Key, loop) &&
                                type.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                        case (Coloring.Off, Coloring.Off) :
                            if (type.ProcessWeakInferenceLoop(strategyManager, entry.Key, loop) &&
                                type.Strategy!.OnCommitBehavior == OnCommitBehavior.Return) return;
                            break;
                    }
                }
            }
        }
    }
}