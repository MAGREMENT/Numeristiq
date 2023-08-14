using System.Collections.Generic;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV1<T> : IAlternatingChainAlgorithm<T> where T : ILoopElement, ILinkGraphElement
{
    private readonly int _maxLoopSize;
    private readonly HashSet<Loop<T>> _loopsProcessed = new();

    public AlternatingChainAlgorithmV1(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        _loopsProcessed.Clear();
        foreach (var start in graph.EachVerticesWith(LinkStrength.Strong))
        {
            Search(graph, new LoopBuilder<T>(start), chainType, view);
        }
    }

    private void Search(LinkGraph<T> graph, LoopBuilder<T> path, IAlternatingChainType<T> chainType, IStrategyManager view)
    {
        if (path.Count > _maxLoopSize) return;
        var last = path.LastElement();

        if (path.Count % 2 == 1)
        {
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    if (path.Count >= 4) chainType.ProcessStrongInference(view, path.FirstElement(), path.End(LinkStrength.Strong));
                }
                else if(!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Strong), chainType, view);
            }
        }
        else
        {
            if (path.Count >= 4)
            {
                var weakFromFirst = graph.GetLinks(path.FirstElement(), LinkStrength.Weak);
                foreach (var weakFromLast in graph.GetLinks(last, LinkStrength.Weak))
                {
                    if (weakFromFirst.Contains(weakFromLast))
                    {
                        if(path.IsAlreadyPresent(weakFromLast)) continue;
                        chainType.ProcessWeakInference(view, weakFromLast, path.Add(weakFromLast, LinkStrength.Weak).End(LinkStrength.Weak));
                    }
                }
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
            {
                if (path.FirstElement().Equals(friend))
                {
                    var loop = path.End(LinkStrength.Weak);
                    if (_loopsProcessed.Contains(loop)) continue;
                    if (path.Count >= 4) chainType.ProcessFullLoop(view, loop);
                    _loopsProcessed.Add(loop);
                }
                else if (!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
            }
            
            foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
            {
                if (path.FirstElement().Equals(friend))
                {
                    var loop = path.End(LinkStrength.Weak);
                    if (_loopsProcessed.Contains(loop)) continue;
                    if (path.Count >= 4) chainType.ProcessFullLoop(view, loop);
                    _loopsProcessed.Add(loop);
                }
                else if (!path.IsAlreadyPresent(friend)) Search(graph, path.Add(friend, LinkStrength.Weak), chainType, view);
            }
        }
    }
}