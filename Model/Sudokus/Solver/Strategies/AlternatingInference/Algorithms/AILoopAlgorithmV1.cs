using System.Collections.Generic;
using Model.Core.Graphs;
using Model.Sudokus.Solver.Utility.Graphs;

namespace Model.Sudokus.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV1<T> : IAlternatingInferenceAlgorithm<T> where T : ISudokuElement
{
    private readonly int _maxLoopSize;
    private readonly HashSet<Loop<T, LinkStrength>> _loopsProcessed = new();

    public AlgorithmType Type => AlgorithmType.Loop;
    
    public AILoopAlgorithmV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(ISudokuSolverData solverData, IAlternatingInferenceType<T> type)
    {
        var graph = type.GetGraph(solverData);
        _loopsProcessed.Clear();
        foreach (var start in graph)
        {
            Search(graph, new ChainBuilder<T, LinkStrength>(start), type, solverData);
        }
    }

    private void Search(ILinkGraph<T> graph, ChainBuilder<T, LinkStrength> path, IAlternatingInferenceType<T> inferenceType, ISudokuSolverData view)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    if (path.Count >= 4) inferenceType.ProcessStrongInferenceLoop(view, path.FirstElement(), path.ToLoop(LinkStrength.Strong));
                }
                else if (!path.ContainsElement(friend))
                {
                    path.Add(LinkStrength.Strong, friend);
                    Search(graph, path, inferenceType, view);
                    path.RemoveLast();
                }
            }
        }
        else
        {
            if (path.Count >= 4)
            {
                foreach (var weakFromLast in graph.Neighbors(last, LinkStrength.Weak))
                {
                    if (graph.AreNeighbors(path.FirstElement(), weakFromLast, LinkStrength.Weak))
                    {
                        if(path.ContainsElement(weakFromLast)) continue;

                        path.Add(LinkStrength.Weak, weakFromLast);
                        inferenceType.ProcessWeakInferenceLoop(view, weakFromLast, path.ToLoop(LinkStrength.Weak));
                        path.RemoveLast();
                    }
                }
            }
            
            foreach (var friend in graph.Neighbors(last, LinkStrength.Weak))
            {
                if (path.FirstElement().Equals(friend))
                {
                    var loop = path.ToLoop(LinkStrength.Weak);
                    if (_loopsProcessed.Contains(loop)) continue;
                    if (path.Count >= 4) inferenceType.ProcessFullLoop(view, loop);
                    _loopsProcessed.Add(loop);
                }
                else if (!path.ContainsElement(friend))
                {
                    path.Add(LinkStrength.Weak, friend);
                    Search(graph, path, inferenceType, view);
                    path.RemoveLast();
                }
            }
            
            foreach (var friend in graph.Neighbors(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    var loop = path.ToLoop(LinkStrength.Weak);
                    if (_loopsProcessed.Contains(loop)) continue;
                    if (path.Count >= 4) inferenceType.ProcessFullLoop(view, loop);
                    _loopsProcessed.Add(loop);
                }
                else if (!path.ContainsElement(friend))
                {
                    path.Add(LinkStrength.Weak, friend);
                    Search(graph, path, inferenceType, view);
                    path.RemoveLast();
                }
            }
        }
    }
}