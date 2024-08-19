using Model.Core.Graphs;
using Model.Core.Graphs.Coloring;
using Model.Core.Graphs.Coloring.ColoringResults;
using Model.Sudokus.Solver.Utility.Graphs;
using Model.Utility;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV3<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    public AlgorithmType Type => AlgorithmType.Loop;
    
    public void Run(ISudokuSolverData solverData, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(solverData);
        
        foreach (var start in graph)
        {
            if (start is not CellPossibility) continue;
            var onColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, ElementColor.On, true);
            var offColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, ElementColor.Off, true);

            foreach (var entry in onColoring)
            {
                if (offColoring.TryGetColoredElement(entry.Key, out var coloring))
                {
                    var (path1, isMono1) = onColoring.History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, entry.Value, graph);
                    var (path2, isMono2) = offColoring.History!.GetPathToRootWithGuessedLinksAndMonoCheck(entry.Key, coloring, graph);
                    if(path2.Count < 2 || path1.Count < 2 || path1.Count + path2.Count < 6) continue;
                    
                    var loop = path1.TryMakeLoop(isMono1, path2, isMono2);
                    if (loop is null) continue;

                    switch (entry.Value, coloring)
                    {
                        case (ElementColor.On, ElementColor.Off) :
                        case (ElementColor.Off, ElementColor.On) :
                            if (type.ProcessFullLoop(solverData, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                        case(ElementColor.On, ElementColor.On) :
                            if (type.ProcessStrongInferenceLoop(solverData, entry.Key, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                        case (ElementColor.Off, ElementColor.Off) :
                            if (type.ProcessWeakInferenceLoop(solverData, entry.Key, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                    }
                }
            }
        }
    }
}