using Model.SudokuSolving.Solver.StrategiesUtility;
using Model.SudokuSolving.Solver.StrategiesUtility.CellColoring;
using Model.SudokuSolving.Solver.StrategiesUtility.CellColoring.ColoringResults;
using Model.SudokuSolving.Solver.StrategiesUtility.Graphs;

namespace Model.SudokuSolving.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV3<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    public AlgorithmType Type => AlgorithmType.Loop;
    
    public void Run(IStrategyManager strategyManager, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(strategyManager);
        
        foreach (var start in graph)
        {
            if (start is not CellPossibility) continue;
            var onColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.On, true);
            var offColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.Off, true);

            foreach (var entry in onColoring)
            {
                if (offColoring.TryGetColoredElement(entry.Key, out var coloring))
                {
                    var path1 = onColoring.History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, entry.Value, graph);
                    var path2 = offColoring.History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, coloring, graph);
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