using Model.Sudokus.Solver.Utility.CellColoring;
using Model.Sudokus.Solver.Utility.CellColoring.ColoringResults;
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
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.On, true);
            var offColoring = ColorHelper.ColorFromStart<T, ColoringDictionary<T>>(
                ColorHelper.Algorithm.ColorWithRules, graph, start, Coloring.Off, true);

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
                        case (Coloring.On, Coloring.Off) :
                        case (Coloring.Off, Coloring.On) :
                            if (type.ProcessFullLoop(solverData, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                        case(Coloring.On, Coloring.On) :
                            if (type.ProcessStrongInferenceLoop(solverData, entry.Key, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                        case (Coloring.Off, Coloring.Off) :
                            if (type.ProcessWeakInferenceLoop(solverData, entry.Key, loop) &&
                                type.Strategy!.StopOnFirstCommit) return;
                            break;
                    }
                }
            }
        }
    }
}