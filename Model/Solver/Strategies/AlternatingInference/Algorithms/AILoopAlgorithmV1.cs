using System.Collections.Generic;
using Global.Enums;
using Model.Solver.StrategiesUtility.Graphs;

namespace Model.Solver.Strategies.AlternatingInference.Algorithms;

public class AILoopAlgorithmV1<T> : IAlternatingInferenceAlgorithm<T> where T : ILinkGraphElement
{
    private readonly int _maxLoopSize;
    private readonly HashSet<LinkGraphLoop<T>> _loopsProcessed = new();

    public AlgorithmType Type => AlgorithmType.Loop;
    
    public AILoopAlgorithmV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(IStrategyManager strategyManager, LinkGraph<T> graph, IAlternatingInferenceType<T> type)
    {
        _loopsProcessed.Clear();
        foreach (var start in graph)
        {
            Search(graph, new LinkGraphChainBuilder<T>(start), type, strategyManager);
        }
    }

    private void Search(LinkGraph<T> graph, LinkGraphChainBuilder<T> path, IAlternatingInferenceType<T> inferenceType, IStrategyManager view)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
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
                foreach (var weakFromLast in graph.GetLinks(last, LinkStrength.Weak))
                {
                    if (graph.HasLinkTo(path.FirstElement(), weakFromLast, LinkStrength.Weak))
                    {
                        if(path.ContainsElement(weakFromLast)) continue;

                        path.Add(LinkStrength.Weak, weakFromLast);
                        inferenceType.ProcessWeakInferenceLoop(view, weakFromLast, path.ToLoop(LinkStrength.Weak));
                        path.RemoveLast();
                    }
                }
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
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
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
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