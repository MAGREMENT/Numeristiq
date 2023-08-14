using System.Collections.Generic;
using Model.StrategiesUtil;
using Model.StrategiesUtil.LoopFinder;

namespace Model.Strategies.AlternatingChains.ChainAlgorithms;

public class AlternatingChainAlgorithmV2<T> : IAlternatingChainAlgorithm<T> where T : ILoopElement, ILinkGraphElement
{
    private readonly int _maxLoopSize;
    private readonly HashSet<Loop<T>> _loopsProcessed = new();

    public AlternatingChainAlgorithmV2(int maxLoopSize)
    {
        _maxLoopSize = maxLoopSize;
    }

    public void Run(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType)
    {
        Dictionary<T, HashSet<T>> searched = new();
        foreach (var start in graph)
        {
            Search(view, graph, chainType, new LoopBuilder<T>(start), searched);
        }
    }

    private void Search(IStrategyManager view, LinkGraph<T> graph, IAlternatingChainType<T> chainType, LoopBuilder<T> builder,
        Dictionary<T, HashSet<T>> searched)
    {
        if (builder.Count > _maxLoopSize) return;

        var last = builder.LastElement();
        var before = builder.ElementBefore();
        bool isPair = builder.Count % 2 == 0;
        HashSet<T>? alreadySearched = searched.TryGetValue(last, out var a) ? a : null;

        foreach (var friend in graph.GetLinks(last, LinkStrength.Strong))
        {
            if (builder.Count == 1 && alreadySearched is not null && alreadySearched.Contains(friend)) continue;
            if (before is not null && friend.Equals(before)) continue;

            var index = builder.IndexOf(friend);
            var linkStrength = !isPair ? LinkStrength.Strong : LinkStrength.Weak;
            
            if (index == -1)
            {
                Search(view, graph, chainType, builder.Add(friend, linkStrength), searched);

                if (searched.TryGetValue(last, out var to)) to.Add(friend);
                else searched.Add(last, new HashSet<T> {friend});
            }
            else
            {
                var cut = builder.Cut(index);
                if (cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4)
                {
                    if (cut.Count % 2 == 0)
                    {
                        var loop = cut.End(LinkStrength.Weak);
                        if (_loopsProcessed.Contains(loop)) continue;

                        chainType.ProcessFullLoop(view, loop);
                        _loopsProcessed.Add(loop);
                    }
                    else
                    {
                        chainType.ProcessStrongInference(view, cut.FirstElement(), cut.End(LinkStrength.Strong));
                    }
                }
            }
        }
        
        if (builder.Count % 2 == 1 && builder.Count < 4) return;
        foreach (var friend in graph.GetLinks(last, LinkStrength.Weak))
        {
            if (before is not null && friend.Equals(before)) continue;
            
            var index = builder.IndexOf(friend);

            if (index == -1)
            {
                if(builder.Count % 2 == 0) Search(view, graph, chainType, builder.Add(friend, LinkStrength.Weak), searched);
            }
            else
            {
                var cut = builder.Cut(index);
                if (cut.FirstLink() == LinkStrength.Strong && cut.Count >= 4)
                {
                    if (cut.Count % 2 == 0)
                    {
                        var loop = cut.End(LinkStrength.Weak);
                        if (_loopsProcessed.Contains(loop)) continue;

                        chainType.ProcessFullLoop(view, loop);
                        _loopsProcessed.Add(loop);
                    }
                    else
                    {
                        chainType.ProcessWeakInference(view, cut.LastElement(), cut.End(LinkStrength.Weak));
                    }
                }
            }
        }
    }
}